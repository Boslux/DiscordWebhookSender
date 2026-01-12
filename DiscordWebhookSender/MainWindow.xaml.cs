using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiscordWebhookSender
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (System.IO.File.Exists("user_settings.json"))
                {
                    string json = System.IO.File.ReadAllText("user_settings.json");
                    var data = JsonSerializer.Deserialize<SettingsData>(json);
                    if (data != null)
                    {
                        WebhookUrlBox.Text = data.WebhookUrl;
                        LinkBox.Text = data.RssLink;
                        SaveSettingsBox.IsChecked = true;
                    }
                }
            }
            catch { /* Ignore */ }
        }

        private void SaveSettings(string url, string rss)
        {
            try
            {
                var data = new SettingsData { WebhookUrl = url, RssLink = rss };
                string json = JsonSerializer.Serialize(data);
                System.IO.File.WriteAllText("user_settings.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Settings save failed: {ex.Message}");
            }
        }

        private class SettingsData
        {
            public string WebhookUrl { get; set; }
            public string RssLink { get; set; }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string link = LinkBox.Text.Trim();
            string customMessage = MessageInputBox.Text.Trim();
            string webhookUrl = WebhookUrlBox.Text.Trim();

            if (string.IsNullOrEmpty(link))
            {
                StatusText.Text = "Error: RSS Link is required.";
                StatusText.Foreground = Brushes.Red;
                return;
            }

            if (string.IsNullOrEmpty(webhookUrl))
            {
                StatusText.Text = "Error: Webhook URL is required.";
                StatusText.Foreground = Brushes.Red;
                return;
            }

            // Save Settings if Checked
            if (SaveSettingsBox.IsChecked == true)
            {
                SaveSettings(webhookUrl, link);
            }
            else
            {
                // Optional: Delete file if unchecked? For now just don't save updates.
                if (System.IO.File.Exists("user_settings.json"))
                    System.IO.File.Delete("user_settings.json");
            }

            SendButton.IsEnabled = false;
            SendButton.Content = "Processing...";
            StatusText.Text = "Starting automation...";
            StatusText.Foreground = Brushes.Yellow;

            try
            {
                // 1. Fetch RSS Feed (Only once)
                string rssXml = await client.GetStringAsync(link);

                // 2. Parse XML
                var xmlDoc = System.Xml.Linq.XDocument.Parse(rssXml);
                var latestItem = xmlDoc.Descendants("item").FirstOrDefault();

                if (latestItem == null)
                {
                    StatusText.Text = "Error: No items found in RSS.";
                    return;
                }

                string title = latestItem.Element("title")?.Value ?? "No Title";
                string description = latestItem.Element("description")?.Value ?? "No Description";
                string newsLink = latestItem.Element("link")?.Value ?? link;

                // Construct Content
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine($"**{title}**");

                if (!string.IsNullOrEmpty(customMessage))
                {
                    sb.AppendLine(customMessage);
                    sb.AppendLine(); // Spacer
                }

                if (IncludeDescriptionBox.IsChecked == true)
                {
                    // Cleanup Description
                    description = System.Text.RegularExpressions.Regex.Replace(description, "<.*?>", string.Empty);
                    description = System.Net.WebUtility.HtmlDecode(description).Trim();
                    if (description.Length > 250) description = description.Substring(0, 247) + "...";

                    sb.AppendLine(description);
                }

                sb.Append(newsLink);
                string finalContent = sb.ToString();

                // 3. PUBLISH TO DISCORD
                var payload = new { content = finalContent };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    StatusText.Text = "Success: Message sent!";
                    StatusText.Foreground = Brushes.LightGreen;
                    MessageBox.Show("Message sent successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    StatusText.Text = "Failed to send.";
                    StatusText.Foreground = Brushes.Red;
                    MessageBox.Show($"Failed: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                StatusText.Foreground = Brushes.Red;
                MessageBox.Show($"Error: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SendButton.IsEnabled = true;
                SendButton.Content = "Send to Discord";
            }
        }
    }
}