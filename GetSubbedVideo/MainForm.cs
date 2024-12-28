using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using TorrentClient;
using HtmlAgilityPack;
using OSDBnet;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using System.Data.SQLite;
using System.IO.Compression;
using Brotli;

namespace GetSubbedVideo
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            set_initial_gui_before_anything(false, "Aloha.2015");

            webBrowser.Document.Write("<html dir='ltr'><head><title></title><style type = text/css>a.verse_id { font-size: 15px; } tr { background-color: #ffe0b4; } td { font-size: 18px; font-family: David; vertical-align: top; } td.highlighted1 { background-color: #ffff00; } span.quote { font-family: Times;} font.close_match { color: #ff6400; font-weight: bold; } font.exact_match { color: red; font-weight: bold; } #total { font-size: 18px; font-family: David; font-weight: bold; margin-top: 8px; } .row_title_close_match { font-family: cursive; } .row_title_perfect_match { font-family: cursive; font-weight: bold } .remark1 { font-size: 13px; font-style: italic; }</style></head><body style=\"background-color:'#FFFFD0';\"><h2 style='margin-bottom: 0px;'></h2><div dir='ltr'><span style='color:blue; font-size: 22px;'>Results: </span><span id='d'></span></div><table style='width:100%; border-spacing: 6px 10px;' cellspacing='5' cellpadding='4'><tbody id='t'></tbody></table><div id='total' dir='ltr'></div></body></html>"); // <col width='10%'><col width='50%'>
            table_el = webBrowser.Document.GetElementById("t");
        }

        #region Global Variables

        static protected string data_dir = Directory.GetCurrentDirectory() + @"\DATA\";
        string lang = null;
        List<TorrentInfo> ti_list = new List<TorrentInfo>();

        #endregion // Global Variables

        Thread handle_run_done_th;

        private void run_handler(object sender, EventArgs e)
        {
            // invoked when the user clicks the Run button or presses Enter on the Movie name TextBox.
            // run the do_it() as thread.
            handle_run_done_th = new Thread(new ThreadStart(do_it));
            handle_run_done_th.IsBackground = true;
            handle_run_done_th.Start();
        }

        class torrent_info
        {
            public string torrent_url;
            public string filename;
            public long video_file_size_bytes;
            public int seeders;
        }

        private void do_it()
        {
            handle_gui_just_after_clicking_run();

            #region get movie_name

            string movie_name = movie_or_Series_Episode_TextBox.Text;
            movie_name = Regex.Replace(movie_name, @"[\s\.]+", " ").Replace(" ", "+").Trim();
            if (movie_name == "")
            {
                MessageBox.Show("No video name. please enter.");
                return;
            }

            #endregion // get movie_name

            #region get subtitles_pages_info_list
            List<Subtitle> subtitles_pages_info_list = null;
            try
            {
                subtitles_pages_info_list = get_using_osdb_subtitles_pages_list(movie_name);
                if (subtitles_pages_info_list == null || subtitles_pages_info_list.Count == 0)
                {
                    throw new Exception("No results for: " + movie_name);
                }
            }
            catch (Exception e)
            {
                Invoke(new MethodInvoker(delegate
                {
                    outputBestRichTextBox.AppendText("Failed to get osdb subtitles pages list. " + e.Message);
                    scroll_to_bottom(outputBestRichTextBox);
                }));
                return;
            }

            #endregion // get subtitles_pages_info_list

            #region get_torrents_links_from_skytorrents_by_link

            string torrent_link = "https://skytorrents.to/?search=" + movie_name;  // + "&type=video&sort=seeders";
            List<skytorrent_info> torrents_links_list = get_torrents_links_from_skytorrents_by_link(torrent_link);
            if (torrents_links_list == null)
            {
                // Warning.
                return;
            }

            #endregion // get_torrents_links_from_skytorrents_by_link

            #region fill skytorrents_video_info_list
            List<torrent_info> skytorrents_video_info_list = new List<torrent_info>();
            for (int t_i = 0; t_i < torrents_links_list.Count; t_i++)
            {
                int torrent_seeders = torrents_links_list[t_i].seeders;
                if (torrent_seeders == 0)
                {
                    break;
                }
                //
                if (skip_skytorrents_scraping_CheckBox.Checked)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        outputRichTextBox.AppendText($"skytorrents scraping skiped.\r\n");
                        scroll_to_bottom(outputRichTextBox);
                    }));
                    break;
                }
                //
                string torrent_url = torrents_links_list[t_i].torrents_link;
                //
                Invoke(new MethodInvoker(delegate
                {
                    outputRichTextBox.AppendText($"skytorrents scrape: {(t_i + 1)}/{torrents_links_list.Count} seeders: {torrent_seeders}\r\n");
                    scroll_to_bottom(outputRichTextBox);
                }));

                byte[] torrent_ba = null;
                try
                {
                    torrent_ba = get_page_ba(torrent_url);
                }
                catch (Exception e)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        outputBestRichTextBox.AppendText(e.Message + "\r\nAborting.");
                        scroll_to_bottom(outputBestRichTextBox);
                    }));
                    return;
                }
                //
                if (torrent_ba == null)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        outputRichTextBox.AppendText("Warning: Failed to get torrent_url: " + torrent_url + "\r\nIgnoring.\r\n");
                        scroll_to_bottom(outputRichTextBox);
                    }));
                    continue;
                }
                //
                TorrentInfo torrent_info = null;
                bool load_successfuly = false;
                try
                {
                    load_successfuly = TorrentInfo.TryLoad(torrent_ba, out torrent_info);
                }
                catch {}
                if (!load_successfuly)
                {
                    Console.WriteLine("failed to process torrent link: " + torrent_url);
                    continue;
                    throw new Exception();
                }
                for (int f_i = 0; f_i < torrent_info.Files.Count(); f_i++)
                {
                    string filename = torrent_info.Files.ElementAt(f_i).FilePath;
                    string file_ext = filename.Substring(filename.LastIndexOf(".") + 1);
                    switch (file_ext)
                    {
                        //case "html":
                        //case "srt":
                        //case "nfo":
                        //case "txt":
                        //case "jpg":
                        //    continue;
                        case "avi":
                        case "mkv":
                        case "mp4":
                            break;
                        default:
                            continue;
                            // throw new Exception();
                    }
                    filename = filename.Substring(filename.LastIndexOf(@"\") + 1);
                    long video_file_size_bytes = torrent_info.Files.ElementAt(f_i).Length;
                    torrent_info ti2 = new torrent_info();
                    ti2.filename = filename;
                    ti2.torrent_url = torrent_url;
                    ti2.video_file_size_bytes = video_file_size_bytes;
                    ti2.seeders = torrent_seeders;
                    skytorrents_video_info_list.Add(ti2);
                }
            }

            if (skytorrents_video_info_list.Count == 0)
            {
                Invoke(new MethodInvoker(delegate
                {
                    outputBestRichTextBox.AppendText("Failed to get any skytorrent pages from itorrent.org probably because you need to browse to https://itorrents.org/torrent\r\nAborting.");
                    resetButton.Enabled = true;
                    scroll_to_bottom(outputBestRichTextBox);
                }));
                return;
            }

            Invoke(new MethodInvoker(delegate
            {
                skip_skytorrents_scraping_CheckBox.Enabled = false;
                outputRichTextBox.AppendText("Done scraping skytorrents.\r\n\r\n" +
                "Comapring results:\r\n");
                scroll_to_bottom(outputRichTextBox);
                pauseCheckBox.Enabled = true;
            }));

            #endregion // fill skytorrents_video_info_list

            #region compare osdb_video_filename with skytorrents_movie_name

            int min_dist = 100000;
            int the_counter = 0;

            for (int subtitles_page_i = 0; subtitles_page_i < subtitles_pages_info_list.Count; subtitles_page_i++)
            {
                Subtitle subtitles_info = subtitles_pages_info_list[subtitles_page_i];
                //
                List<osdb_info> osdb_movies_infos_list = null;
                try
                {
                    osdb_movies_infos_list = get_osdb_movies_infos_by_subtitle_link(subtitles_info.SubtitlePageLink.ToString());
                }
                catch (Exception e2)
                {
                    // Warning.
                    continue;
                }
                //
                for (int i = 0; i < osdb_movies_infos_list.Count; i++)
                {
                    if (this.pauseCheckBox.Checked)
                    {
                        bool is_reset = handle_pause();
                        if (is_reset)
                        {
                            return; // exit.
                        }
                    }

                    string osdb_video_filename = osdb_movies_infos_list[i].osdb_video_filename;
                    if (!Regex.IsMatch(osdb_video_filename, @"\.(avi|mkv|mp4)$"))
                    {
                        continue;
                        // throw new Exception();
                    }
                    long osdb_video_size_bytes = osdb_movies_infos_list[i].osdb_video_size_bytes;
                    for (int j = 0; j < skytorrents_video_info_list.Count; j++)
                    {
                        string skytorrents_movie_name = skytorrents_video_info_list[j].filename;
                        long skytorrents_movie_size_bytes = skytorrents_video_info_list[j].video_file_size_bytes;
                        string torrent_url = skytorrents_video_info_list[j].torrent_url;
                        int torrent_seeders = skytorrents_video_info_list[j].seeders;
                        //
                        int comp_res = CompareHelper.LevenshteinDistance(osdb_video_filename, skytorrents_movie_name);
                        long size_diff = Math.Abs(osdb_video_size_bytes - skytorrents_movie_size_bytes);
                        //
                        Invoke(new MethodInvoker(delegate
                        {
                            outputRichTextBox.AppendText(++the_counter + "\t" + osdb_video_filename + "\t" + skytorrents_movie_name + "\t" + comp_res + "\r\n");
                            scroll_to_bottom(outputRichTextBox);
                        }));
                        //
                        // Console.WriteLine();
                        if (min_dist > comp_res || comp_res == 0)
                        {
                            min_dist = comp_res;
                            //
                            string skytorrents_movie_size_str = bytes_to_string(skytorrents_movie_size_bytes);
                            string osdb_video_size_str = bytes_to_string(osdb_video_size_bytes);
                            string size_diff_str = bytes_to_string(size_diff);
                            //
                            if (osdb_video_filename != skytorrents_movie_name)
                            {
                                // debug.
                            }
                            //
                            Invoke(new MethodInvoker(delegate
                            {
                                if (outputBestRichTextBox.Text != "")
                                {
                                    outputBestRichTextBox.AppendText("\r\n");
                                }
                                outputBestRichTextBox.AppendText(the_counter + ") Subtitles: " + subtitles_info.SubtitleFileName + "\t" + subtitles_info.SubTitleDownloadLink + "\r\n");
                                outputBestRichTextBox.AppendText("Video: osdb: " + osdb_video_filename + "\tskytorrents: " + skytorrents_movie_name + "\tdistance: " + comp_res + "\r\n");
                                outputBestRichTextBox.AppendText("sizes: " + osdb_video_size_str + "B\t" + skytorrents_movie_size_str + "B" + "\tsize_diff: " + size_diff_str + "B\r\n");
                                outputBestRichTextBox.AppendText(torrent_url + "\tseeders: " + torrent_seeders + "\r\n\r\n");

                                bool is_perfect_match = min_dist == 0 && size_diff == 0;
                                //cell_prop first_cell = new cell_prop();
                                //first_cell.html = the_counter.ToString();
                                /*
                                add_table_row(new cell_prop[] { new cell_prop { html = $"Permutation Number <b>{the_counter.ToString()}</b>", className = "row_title_" + (is_perfect_match ? "perfect" : "close") + "_match" },
                                                                new cell_prop { html = is_perfect_match ? "Perfect Match Found !!" + "<br><span class='remark1'>Identical OpenSubtitles and SkyTorrents Video file names and sizes.</span>" : "Close Match Found" + $"<br><span class='remark1'>Difference of <b>{comp_res}</b> letters in video file names and <b>{size_diff_str}B</b> in video file sizes.</span>" } });
                                //
                                add_table_row(new cell_prop[] { new cell_prop { html = the_counter.ToString(), className = "highlighted1" }, new cell_prop { html = subtitles_info.SubtitleFileName }, new cell_prop { html = subtitles_info.SubTitleDownloadLink.ToString() }, new cell_prop { html = osdb_video_filename }, new cell_prop { html = skytorrents_movie_name }, new cell_prop { html = comp_res.ToString() } });
                                add_table_row(new cell_prop[] { new cell_prop { html = osdb_video_size_str }, new cell_prop { html = skytorrents_movie_size_str }, new cell_prop { html = size_diff_str }, new cell_prop { html = torrent_url }, new cell_prop { html = torrent_seeders.ToString() } });
                                */

                                /* Yishay: */

                                string tr_color_str;
                                if (is_perfect_match)
                                {
                                    tr_color_str = "aqua";
                                }
                                else
                                {
                                    tr_color_str = "azure";
                                }

                                add_table_row(tr_color_str, new cell_prop[] { new cell_prop { html = $"Permutation Number <b>{the_counter.ToString()}</b>", className = "row_title_" + (is_perfect_match ? "perfect" : "close") + "_match" },
                                                                new cell_prop { html = is_perfect_match ? "Perfect Match Found !!" + "<br><span class='remark1'>Identical OpenSubtitles and SkyTorrents Video file names and sizes.</span>" : "Close Match Found" + $"<br><span class='remark1'>Difference of <b>{comp_res}</b> letters in video file names and <b>{size_diff_str}B</b> in video file sizes.</span>" } });

                                //add_table_row(new cell_prop[] { new cell_prop { html = the_counter.ToString(), className = "highlighted1" }, new cell_prop { html = subtitles_info.SubtitleFileName }, new cell_prop { html = subtitles_info.SubTitleDownloadLink.ToString() }, new cell_prop { html = osdb_video_filename }, new cell_prop { html = skytorrents_movie_name }, new cell_prop { html = comp_res.ToString() } });
                                //add_table_row(new cell_prop[] { new cell_prop { html = osdb_video_size_str }, new cell_prop { html = skytorrents_movie_size_str }, new cell_prop { html = size_diff_str }, new cell_prop { html = torrent_url }, new cell_prop { html = torrent_seeders.ToString() } });

                                add_table_row(tr_color_str, new cell_prop[] { new cell_prop { html = $"OpenSubtitles Subtitle Name" }, new cell_prop { html = subtitles_info.SubtitleFileName } });
                                add_table_row(tr_color_str, new cell_prop[] { new cell_prop { html = $"OpenSubtitles Video name" }, new cell_prop { html = $"<b>{osdb_video_size_str}B</b> - {osdb_video_filename}" } });
                                add_table_row(tr_color_str, new cell_prop[] { new cell_prop { html = $"SkyTorrents Video name" }, new cell_prop { html = $"<b>{skytorrents_movie_size_str}B</b> - {skytorrents_movie_name}" } });
                                add_table_row(tr_color_str, new cell_prop[] { new cell_prop { html = $"Subtitle file Download Link" }, new cell_prop { html = $"<a href=\"{subtitles_info.SubTitleDownloadLink.ToString()}\" target=\"_blank\">{subtitles_info.SubTitleDownloadLink.ToString()}</a>" } });
                                add_table_row(tr_color_str, new cell_prop[] { new cell_prop { html = $"Video file Torrent link", className = "highlighted1" }, new cell_prop { html = $"<a href=\"{torrent_url}\" target=\"_blank\">{torrent_url}</a><br><b>{torrent_seeders.ToString()} seeders</b>" } });
                                add_table_row(tr_color_str, new cell_prop[] { new cell_prop { html = $"<br />" } });


                                scroll_to_bottom(outputBestRichTextBox);
                            }));
                        }
                    }
                }
            }

            #endregion // compare osdb_video_filename with skytorrents_movie_name

            Invoke(new MethodInvoker(delegate
            {
                File.WriteAllText("$res.html", webBrowser.Document.Body.Parent.OuterHtml, Encoding.GetEncoding(webBrowser.Document.Encoding));
            }));

            Invoke(new MethodInvoker(delegate
            {
                outputBestRichTextBox.AppendText("Done." + "\r\n");
                scroll_to_bottom(outputBestRichTextBox);
                pauseCheckBox.Enabled = false;
                resetButton.Enabled = true;
            }));

            return;

        }

        HtmlElement table_el = null;

        class cell_prop
        {
            public string html;
            public string className = null;
        }

        static int html_row_id = 0;
        void add_table_row(string tr_color_str, cell_prop[] cells_prop_ar)
        {
            HtmlElement tr_el = webBrowser.Document.CreateElement("tr");
            tr_el.SetAttribute("id", (++html_row_id).ToString());
            tr_el.Click += new HtmlElementEventHandler(htmlDoc_Click);
            tr_el.Style = $"background-color: {tr_color_str};";
            foreach (cell_prop cell_prop in cells_prop_ar)
            {
                HtmlElement td_el = webBrowser.Document.CreateElement("td");
                if (cell_prop.className != null)
                {
                    // td_el.Style = "background-color: '#FFFF00';";
                    td_el.SetAttribute("className", cell_prop.className);
                }
                td_el.InnerHtml = cell_prop.html; //  $"<a class='verse_id' target=\"_blank\" href=\"\">[" + "" + "]</a> \"<span class='quote'>" + col_str + $"</span>\".";
                tr_el.AppendChild(td_el);
                table_el.AppendChild(tr_el);
            }

            // webBrowser.Document.ActiveElement.AppendChild(el);
            scroll_to_bottom_browser();
        }

        private void htmlDoc_Click(object sender, HtmlElementEventArgs e)
        {
            HtmlElement el = (HtmlElement)sender;
            MessageBox.Show("Clicked row: " + el.GetAttribute("id"));
        }

        private void scroll_to_bottom_browser()
        {
            webBrowser.Document.Window.ScrollTo(0, webBrowser.Document.Body.ScrollRectangle.Height);
        }

        #region osdb stuff

        static string GetSystemLanguage()
        {
            var currentCulture = System.Globalization.CultureInfo.CurrentUICulture;
            return currentCulture.TwoLetterISOLanguageName.ToLower();
        }

        private List<Subtitle> get_using_osdb_subtitles_pages_list(string movie_name)
        {
            string sl = GetSystemLanguage();
            IAnonymousClient osdb = Osdb.Login("", "TemporaryUserAgent");
            IList<Subtitle> full_res = osdb.SearchSubtitlesFromQuery(lang, movie_name);
            List<Subtitle> res = new List<Subtitle>();
            for (int i = 0; i < full_res.Count; i++)
            {
                res.Add(full_res[i]);
            }
            return res;
            // IEnumerable<MovieInfo> a = osdb.CheckMovieHash("488aedcffb8f40f53338740b1e46c4d5");
        }

        public class osdb_info
        {
            public string osdb_video_filename;
            public long osdb_video_size_bytes;
        }

        public List<osdb_info> get_osdb_movies_infos_by_subtitle_link(string subtitle_link)
        {
            // url = "https://www.opensubtitles.org/he/subtitles/3511986/the-matrix-reloaded-he";
            string html = get_page_html(subtitle_link);
            //
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            //
            List<osdb_info> osdb_videos_info_list = new List<osdb_info>();
            HtmlNodeCollection _els = doc.DocumentNode.SelectNodes(@"//a[contains(@title, 'Download -')]");
            Hashtable osdb_videos_filename_ht = new Hashtable();
            for (int i = 0; i < _els.Count; i++)
            {
                HtmlNode _el = _els[i];
                string osdb_video_filename = _el.InnerText;
                long video_size_bytes = long.Parse(_el.SelectSingleNode(@"./following-sibling::a").InnerText);
                if (osdb_videos_filename_ht.Contains(osdb_video_filename))
                {
                    // debug.
                    continue;
                }
                osdb_videos_filename_ht.Add(osdb_video_filename, null);
                //
                osdb_info oi = new osdb_info();
                oi.osdb_video_filename = osdb_video_filename;
                oi.osdb_video_size_bytes = video_size_bytes;
                osdb_videos_info_list.Add(oi);
            }
            return osdb_videos_info_list;
        }

        #endregion // osdb stuff

        #region skytorrent stuff

        public class skytorrent_info
        {
            public string torrents_link;
            public int seeders;
        }

        public List<skytorrent_info> get_torrents_links_from_skytorrents_by_link(string skytorrents_link)
        {
            string html = null;
            try
            {
                html = get_page_html(skytorrents_link, false);
            }
            catch (Exception e)
            {
                Invoke(new MethodInvoker(delegate
                {
                    outputBestRichTextBox.AppendText($"Failed to read skytorrents page : {skytorrents_link}.\r\nAborting.");
                    resetButton.Enabled = true;
                    scroll_to_bottom(outputBestRichTextBox);
                }));
                return null;
            }
            //
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            //
            List<skytorrent_info> torrents_info_list = new List<skytorrent_info>();
            //
            //HtmlNodeCollection _els = doc.DocumentNode.SelectNodes(@"//tr[@class='result']/td/a[contains(@href, 'itorrents.org')]"); //skytorrents-download.webefir.com
            HtmlNodeCollection _els = doc.DocumentNode.SelectNodes(@"//tr[@class='result']/td/a[contains(@href, '.torrent')]"); //skytorrents-download.webefir.com
            if (_els == null)
            {
                Invoke(new MethodInvoker(delegate
                {
                    outputBestRichTextBox.AppendText($"Couldn't find any Torrents Links in: {skytorrents_link}.\r\nAborting.");
                    scroll_to_bottom(outputBestRichTextBox);
                }));
                return null;
            }
            //
            for (int i = 0; i < _els.Count; i++)
            {
                HtmlNode _el = _els[i];
                string skytorrents_download_webefir_com_link = _el.Attributes["href"].Value;
                //
                string link_str = skytorrents_download_webefir_com_link; // "http://skytorrents.to/" + 
                html = get_page_html(link_str);
                HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                doc2.LoadHtml(html);
                //HtmlNode _el2 = doc2.DocumentNode.SelectSingleNode(@"//input[contains(@src,'downld.png')]/ancestor::div[1]");
                HtmlNode _el2 = doc2.DocumentNode.SelectSingleNode(@"//a[contains(@href, '.torrent')]");
                string itorrents_link = _el2.Attributes["href"].Value;
                //string itorrents_key = _el2.SelectSingleNode(@".//input[@type='hidden']").Attributes["value"].Value;
                //string itorrents_link = "https://itorrents.org/torrent/" + itorrents_key + ".torrent";
                //
                HtmlNode _el3 = _el.SelectSingleNode(@"../following-sibling::td[contains(@style,'color:green')]");
                int seeders = int.Parse(_el3.InnerText.Replace(",", ""));
                skytorrent_info sti = new skytorrent_info();
                sti.torrents_link = itorrents_link;
                sti.seeders = seeders;
                torrents_info_list.Add(sti);
            }
            //
            return torrents_info_list;
        }

        #endregion // skytorrent stuff

        #region Chrome

        private static string GetChromeCookiePath()
        {
            string s = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\cookies";

            if (!File.Exists(s))
            {
                throw new Exception("Chrome Cookies file not found at: " + s);
            }

            return s;
        }


        // chrome://settings/content/all
        // chrome://settings/cookies/detail?site=itorrents.org

        private Hashtable GetCookie_Chrome(string host_str)
        {
            string host_without_www_prefix_str = Regex.Replace(host_str, @"^www\.", "");
            string strPath = GetChromeCookiePath();
            string strDb = "Data Source=" + strPath + ";pooling=false";
            SQLiteConnection conn = new SQLiteConnection(strDb);
            conn.Open();
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT name,encrypted_value,host_key FROM cookies WHERE host_key LIKE '%" + host_without_www_prefix_str + "%'"; // AND name LIKE '%" + strField + "%';";
            SQLiteDataReader reader = cmd.ExecuteReader();
            Hashtable cookies_ht = new Hashtable();
            while (reader.Read())
            {
                string name = (string)reader[0];
                var encryptedValue = (byte[])reader[1];
                string value = null;
                try
                {
                    var decryptedValue = System.Security.Cryptography.ProtectedData.Unprotect(encryptedValue, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    value = Encoding.ASCII.GetString(decryptedValue);
                }
                catch (Exception e)
                {
                    if (name == "cf_clearance")
                    {
                        value = "5fbc887450ba62af27f4deae9b54b2a505013758-1587417642-0-150";
                    }
                    else if (name == "__cfduid")
                    {
                        value = "dcb33c23c802e58f844d37739001e1fda1585764269";
                    }
                    else if (name == "_ga")
                    {
                        value = "GA1.2.1163655300.1547458073";
                    }
                    else
                    {
                        Invoke(new MethodInvoker(delegate
                        {
                            outputRichTextBox.AppendText("Warning: failed to read Cookie name: " + name + "\r\n");
                            scroll_to_bottom(outputRichTextBox);
                        }));
                        continue;
                        // throw e;
                    }
                }

                //
                string chrome_host_str = (string)reader[2];
                if (!host_str.Contains(chrome_host_str.TrimStart('.')))
                {
                    continue;
                }
                cookies_ht.Add(name, value);
            }
            conn.Close();
            return cookies_ht;
        }

        #endregion //Chrome

        #region Web stuff

        private string get_page_html(string url, bool use_chrome_cookies = true)
        {
            Uri uri = new Uri(url);
            //
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            //request.ContentType = "text/html; charset=UTF-8";
            request.UserAgent = get_user_agent();
            //
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            request.Headers.Add("accept-encoding", "gzip, deflate, br");
            //request.Headers.Add("accept-language", "he-IL,he;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Referer = url;
            request.KeepAlive = true;
            //
            Hashtable cookies_ht;
            if (use_chrome_cookies)
            {
                cookies_ht = GetCookie_Chrome(uri.Host);
            }
            else
            {
                cookies_ht = new Hashtable();
            }
            CookieContainer cc = new CookieContainer();
            foreach (string key in cookies_ht.Keys)
            {
                Cookie c = new Cookie(key, (string)cookies_ht[key]);
                cc.Add(new Uri(uri.Scheme + "://" + uri.Host), c);
            }
            request.CookieContainer = cc;
            //
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //
            Stream response_stream = response.GetResponseStream();
            //
            MemoryStream ms = new MemoryStream();
            //
            if (response.ContentEncoding == "br")
            {
                BrotliStream decompressionStream = new BrotliStream(response_stream, CompressionMode.Decompress);
                CopyStreamToStream(decompressionStream, ms);
                decompressionStream.Close();
            }
            else
            {
                CopyStreamToStream(response_stream, ms);
            }
            //
            response_stream.Close();
            //
            byte[] ba = ms.ToArray();
            //
            string html = Encoding.GetEncoding(response.CharacterSet).GetString(ba);
            //
            return html;
            //
            // ms.Position = 0;
            // StreamReader reader = new StreamReader(response_stream, Encoding.UTF8);
            // string html = reader.ReadToEnd();
            // return html;
        }

        private byte[] get_page_ba(string torrent_url)
        {
            byte[] torrent_ba = null;
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                client.Headers.Add("accept-encoding", "gzip, deflate, br");
                client.Headers.Add("accept-language", "he-IL,he;q=0.9,en-US;q=0.8,en;q=0.7");
                client.Headers.Add("cache-control", "no-cache");
                var ua = get_user_agent();
                client.Headers.Add("user-agent", ua);
                client.Headers.Add("upgrade-insecure-requests", "1");

                Hashtable cookies_ht = GetCookie_Chrome(@"itorrents.org");
                if (cookies_ht.Count == 0)
                {
                    throw new Exception("no cookies found in: itorrents.org");
                }
                string cookie_str = "";
                foreach (string name in cookies_ht.Keys)
                {
                    cookie_str += name + "=" + cookies_ht[name] + ";";
                }
                client.Headers.Add("cookie", cookie_str);

                torrent_ba = client.DownloadData(torrent_url);
            }
            catch (Exception e1)
            {
                Invoke(new MethodInvoker(delegate
                {
                    outputRichTextBox.AppendText("Warning: failed to load itorrents.org url: " + torrent_url + " " + e1.Message + "\r\n");
                    scroll_to_bottom(outputRichTextBox);
                }));
                //throw new Exception("failed to load itorrents.org url: " + torrent_url + " " + e1.Message);
                return null;
            }
            //
            return torrent_ba;
        }

        public static long CopyStreamToStream(Stream source, Stream destination)
        {
            byte[] buffer = new byte[2048];
            int bytesRead;
            long totalBytes = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                totalBytes += bytesRead;
            }
            return totalBytes;
        }

        private static string get_user_agent()
        {
            string user_agent = null;
            string user_agent_file_path = data_dir + "user_agent.txt";
            string[] user_agent_lines_str = File.ReadAllLines(user_agent_file_path);
            for (int i = 0; i < user_agent_lines_str.Length; i++)
            {
                string line = user_agent_lines_str[i];
                if (line.StartsWith("#"))
                {
                    continue;
                }
                user_agent = line;
                break;
            }

            return user_agent;
        }

        private static string bytes_to_string(long n_bytes)
        {
            string size_str;
            if (n_bytes < 1024)
            {
                size_str = n_bytes + " ";
            }
            else if (n_bytes < 1024 * 1024)
            {
                size_str = ((float)n_bytes / 1024).ToString("0.0") + " K";
            }
            else if (n_bytes < 1024 * 1024 * 1024)
            {
                size_str = ((float)n_bytes / 1024 / 1024).ToString("0.0") + " M";
            }
            else
            {
                size_str = ((float)n_bytes / 1024 / 1024 / 1024).ToString("0.0") + " G";
            }

            return size_str;
        }

        #endregion // Web stuff

        #region TorrentClient

        TorrentClient.TorrentClient torrent_client = null;
        int listeningPort = 4000; // must be available and between 1025 and 65535

        public void start_torrent_client()
        {
            torrent_client = new TorrentClient.TorrentClient(listeningPort, data_dir + "Torrent_Data");
            torrent_client.DownloadSpeedLimit = 100 * 1024; // 100 KB/s
            torrent_client.UploadSpeedLimit = 200 * 1024; // 200 KB/s
            torrent_client.TorrentHashing += this.TorrentClient_TorrentHashing;
            torrent_client.TorrentLeeching += this.TorrentClient_TorrentLeeching;
            torrent_client.TorrentSeeding += this.TorrentClient_TorrentSeeding;
            torrent_client.TorrentStarted += this.TorrentClient_TorrentStarted;
            torrent_client.TorrentStopped += this.TorrentClient_TorrentStopped;
            torrent_client.Start(); // start torrent client
        }

        public TorrentInfo download_movie(string torrent_file_name)
        {
            if (torrent_client == null)
            {
                throw new Exception("torrent_client not started.");
            }
            string torrentInfoFilePath = data_dir + @"Torren_Info_Files\" + torrent_file_name;
            TorrentInfo torrent_info;
            bool load_successfuly = TorrentInfo.TryLoad(torrentInfoFilePath, out torrent_info);
            if (load_successfuly)
            {
                torrent_client.Start(torrent_info); // start torrent file
            }
            else
            {
                throw new Exception("loading TorrentInfo failed.");
            }
            return torrent_info;
        }

        private void TorrentClient_TorrentHashing(object sender, TorrentHashingEventArgs e)
        {
            // occurs when a torrent's pieces are being hashed
            Console.WriteLine($"hashing {e.TorrentInfo.InfoHash}");
        }

        private void TorrentClient_TorrentLeeching(object sender, TorrentLeechingEventArgs e)
        {
            // occurs when a torrent is being leeched (pieces are being downloaded)
            Console.WriteLine($"leeching {e.TorrentInfo.InfoHash}");
            // the folowing 2 lines, added by Aryeh:
            long downloaded_bytes = ((TorrentClient.TorrentClient)sender).Downloaded;
            Console.WriteLine($"Downloaded: {downloaded_bytes} of {e.TorrentInfo.Length} ({(((float)downloaded_bytes * 100 / e.TorrentInfo.Length)).ToString("0.00")}%)");
        }

        private void TorrentClient_TorrentSeeding(object sender, TorrentSeedingEventArgs e)
        {
            // occurs when a torrent is being leeched (pieces are being uploaded)
            Console.WriteLine($"seeding {e.TorrentInfo.InfoHash}");
        }

        private void TorrentClient_TorrentStarted(object sender, TorrentStartedEventArgs e)
        {
            // occurs when a torrent is started
            Console.WriteLine($"started {e.TorrentInfo.InfoHash}");
        }

        private void TorrentClient_TorrentStopped(object sender, TorrentStoppedEventArgs e)
        {
            // occurs when a torrent is stopped
            Console.WriteLine($"stopped {e.TorrentInfo.InfoHash}");
        }

        #endregion // TorrentClient 

        #region GUI stuff

        private void set_initial_gui_before_anything(bool is_cross_thread, string movie_or_Series_Episode = null)
        {
            if (is_cross_thread)
            {
                Invoke(new MethodInvoker(delegate
                {
                    do_set_initial_gui_before_anything(movie_or_Series_Episode);
                }));
            }
            else
            {
                do_set_initial_gui_before_anything(movie_or_Series_Episode);
            }
        }

        private void do_set_initial_gui_before_anything(string movie_or_Series_Episode)
        {
            lang_ComboBox.SelectedIndex = 0;
            if (movie_or_Series_Episode != null)
            {
                movie_or_Series_Episode_TextBox.Text = movie_or_Series_Episode;
            }
            pauseCheckBox.Enabled = false;
            resetButton.Enabled = false;
            skip_skytorrents_scraping_CheckBox.Enabled = false;
            //
            movie_or_Series_Episode_TextBox.Enabled = true;
            pauseCheckBox.Checked = false;
            lang_ComboBox.Enabled = true;
            outputRichTextBox.Text = "";
            outputBestRichTextBox.Text = "";
            run_Button.Enabled = true;
            pauseCheckBox.BackColor = System.Drawing.SystemColors.Control;
            skip_skytorrents_scraping_CheckBox.Checked = false;
            //
            resetButton_Clicked = false;
        }

        private void handle_gui_just_after_clicking_run()
        {
            Invoke(new MethodInvoker(delegate
            {
                movie_or_Series_Episode_TextBox.Enabled = false;
                run_Button.Enabled = false;
                lang = (string)lang_ComboBox.SelectedItem;
                lang_ComboBox.Enabled = false;
                skip_skytorrents_scraping_CheckBox.Enabled = true;
            }));
        }

        protected bool handle_pause()
        {
            Invoke(new MethodInvoker(delegate
            {
                resetButton.Enabled = true;
            }));
            pauseCheckBox.BackColor = System.Drawing.Color.Pink;
            while (true)
            {
                if (resetButton_Clicked)
                {
                    return true;
                }
                if (!pauseCheckBox.Checked)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        resetButton.Enabled = false;
                    }));
                    break;
                }
                Thread.Sleep(500);
            }
            pauseCheckBox.BackColor = System.Drawing.SystemColors.Control;
            return false;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            foreach (TorrentInfo ti in ti_list)
            {
                torrent_client.Stop(ti.InfoHash);
            }
            // torrent_client.Stop();
        }

        private void handle_Link_Clicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void key_pressed_on_movie_or_Series_Episode_TextBox(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                run_handler(sender, e);
            }
        }

        private void scroll_to_bottom(RichTextBox tb)
        {
            tb.SelectionStart = tb.Text.Length;
            tb.ScrollToCaret();
        }

        static bool resetButton_Clicked = false;

        private void resetButton_Click(object sender, EventArgs e)
        {
            resetButton_Clicked = true;
            handle_run_done_th.Join();
            set_initial_gui_before_anything(true);
        }

        #endregion // GUI stuff
    }
}