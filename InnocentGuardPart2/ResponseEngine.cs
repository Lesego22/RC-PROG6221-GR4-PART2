using System;
using System.Collections.Generic;

namespace InnocentGuardPart2
{
 
    public delegate string ResponseDelegate(string input);

    public delegate string SentimentDelegate(string input);

    class ResponseEngine
    {
        private SentimentDetector sentimentDetector = new SentimentDetector();
        private ChatMemory memory = new ChatMemory();
        private string lastTopic = string.Empty;
        private Random random = new Random();

        
        private ResponseDelegate getKeywordResponse;
        private ResponseDelegate getRandomResponse;
        private SentimentDelegate detectSentiment;

        //  Fixed responses for each keyword
        private Dictionary<string, string> keywordResponses = new Dictionary<string, string>()
        {
            { "password",
              "Passwords should be at least 12 characters and include uppercase, lowercase, numbers, and symbols. " +
              "Never reuse the same password across different sites. A password manager like Bitwarden is a great help!" },

            { "safe browsing",
              "To browse safely: always look for HTTPS in the URL bar, avoid public Wi-Fi for banking, " +
              "keep your browser updated, and never download files from sources you don't trust." },

            { "malware",
              "Malware is harmful software designed to steal your data or damage your device. " +
              "Install a trusted antivirus, keep everything updated, and never open attachments from unknown senders." },

            { "two-factor",
              "Two-factor authentication (2FA) adds a second layer of security beyond just a password. " +
              "Even if someone steals your password, they still can't get in without the second code sent to your phone." },

            { "2fa",
              "Two-factor authentication (2FA) is one of the best things you can do for your accounts. " +
              "Enable it on your email, banking, and social media accounts right away!" },

            { "social engineering",
              "Social engineering is when hackers manipulate people rather than systems. " +
              "They might pretend to be IT support or your bank. Never give out passwords over the phone." },

            { "vpn",
              "A VPN (Virtual Private Network) hides your internet traffic from hackers. " +
              "It's especially useful on public Wi-Fi. Good options include NordVPN and ProtonVPN." },

            { "scam",
              "Scams are everywhere online — fake job offers, lottery wins, and urgent bank alerts. " +
              "If something sounds too good to be true, it almost always is." },

            { "privacy",
              "Protecting your privacy starts with reviewing app permissions, using strong passwords, " +
              "enabling 2FA, and being careful about what personal info you share on social media." },

            { "how are you",
              "I'm doing great and always on guard against cyber threats! How can I help you today?" },

            { "what is your purpose",
              "I'm InnocentGuard — your personal cybersecurity assistant. I help South Africans stay safe online." },

            { "help",
              "Here are the topics I can help you with:\n\n" +
              "  • password\n  • phishing\n  • safe browsing\n  • malware\n  • vpn\n" +
              "  • two-factor authentication (2fa)\n  • social engineering\n  • scam\n  • privacy\n\n" +
              "Just type any of those, or ask me anything!" },
        };

        private Dictionary<string, List<string>> randomResponses = new Dictionary<string, List<string>>()
        {
            {
                "phishing", new List<string>
                {
                    "Phishing emails often create urgency — 'Your account will be closed!' " +
                    "Always pause and verify before clicking any link.",

                    "A common phishing trick is using email addresses that look almost right, " +
                    "like 'support@paypa1.com' instead of 'support@paypal.com'. Always check carefully.",

                    "If you get an email asking you to log in, don't click the link. " +
                    "Open a new browser tab and go directly to the website yourself.",

                    "Legitimate banks will NEVER ask for your password via email or SMS. " +
                    "If you receive such a message, it is almost certainly a phishing attempt."
                }
            },
        };

        public ResponseEngine()
        {
            

            getKeywordResponse = LookupKeywordResponse;   
            getRandomResponse = LookupRandomResponse;    
            detectSentiment = sentimentDetector.Detect; 
        }

        public void SetUserName(string name)
        {
            memory.Remember("name", name);
        }

        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "I didn't catch that — could you type something so I can help you?";

            string lower = input.ToLower().Trim();
            string userName = memory.Recall("name");
            string prefix = string.IsNullOrEmpty(userName) ? "" : $"{userName}, ";

            // Handle follow-up questions using memory of last topic
            if (lower.Contains("more") || lower.Contains("explain") || lower.Contains("another tip"))
            {
                if (!string.IsNullOrEmpty(lastTopic))
                {
                    // Use the delegate to call the right response method
                    string followUp = getKeywordResponse(lastTopic);
                    if (followUp.StartsWith("Sorry"))
                        followUp = getRandomResponse(lastTopic);
                    return prefix + followUp;
                }
                return "Sure! Which topic would you like to know more about? " +
                       "Try: password, phishing, malware, or VPN.";
            }

            string detectedSentiment = detectSentiment(lower);
            string empathyPrefix = GetEmpathyPrefix(detectedSentiment, prefix);

            string keywordResult = getKeywordResponse(lower);
            if (!keywordResult.StartsWith("Sorry"))
                return empathyPrefix + keywordResult;

            string randomResult = getRandomResponse(lower);
            if (!randomResult.StartsWith("Sorry"))
                return empathyPrefix + randomResult;

            return $"{prefix}I didn't quite understand that. " +
                   "Type 'help' to see all the topics I can assist with.";
        }

       

        private string LookupKeywordResponse(string input)
        {
            
            foreach (var entry in keywordResponses)
            {
                if (input.Contains(entry.Key))
                {
                    lastTopic = entry.Key;
                    if (entry.Key != "how are you" && entry.Key != "help" && entry.Key != "what is your purpose")
                        memory.Remember("topic", entry.Key);
                    return entry.Value;
                }
            }
            return "Sorry, no keyword match found.";
        }

        private string LookupRandomResponse(string input)
        {
            
            foreach (var entry in randomResponses)
            {
                if (input.Contains(entry.Key))
                {
                    lastTopic = entry.Key;
                    memory.Remember("topic", entry.Key);
                    int index = random.Next(entry.Value.Count);
                    return entry.Value[index];
                }
            }
            return "Sorry, no random response found.";
        }

        private string GetEmpathyPrefix(string detectedSentiment, string prefix)
        {
            switch (detectedSentiment)
            {
                case "worried":
                    return $"{prefix}It's completely understandable to feel that way — " +
                           "cyber threats are real. Let me help you stay protected. ";
                case "frustrated":
                    return $"{prefix}I understand this can feel overwhelming. " +
                           "Let me explain this as clearly as possible. ";
                case "curious":
                    return $"{prefix}Great question! I love that you're eager to learn. ";
                default:
                    return string.Empty;
            }
        }
    }
}