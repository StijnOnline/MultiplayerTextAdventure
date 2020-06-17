using Assets.Code.Client;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using TMPro;
using UnityEngine;

namespace Assets.Code {

    public class Scores : MonoBehaviour {

        public TextMeshProUGUI lastGameScores;
        public TextMeshProUGUI topWinners;
        public TextMeshProUGUI topSecond;
        public TextMeshProUGUI monthlyPlays;
        public TextMeshProUGUI failed;

        private const string databaseLoginUsername = "TestServer";
        private const string databaseLoginPass = "onsyckerabi";

        private string SessionID;
        private int getStatisticsAttempts = 0;

        //not great implementation
        public void SetLastGameScores(Lobby lobby, List<EndGameMessage.ScoreData> scores) {
            //should be sent already sorted
            for(int i = 0; i < scores.Count; i++) {

                lastGameScores.text += lobby.players[scores[i].playerID].PlayerTextWithColorTag() + " came ";
                switch(i) {
                    case 0: lastGameScores.text += "first"; break;
                    case 1: lastGameScores.text += "second"; break;
                    case 2: lastGameScores.text += "third"; break;
                    case 3: lastGameScores.text += "fourth"; break;
                }


                lastGameScores.text += " with " + scores[i].gold + " gold\n";
            }

        }

        public void OnEnable() {
            getStatisticsAttempts = 0;
            GetStatistics();
        }



        private async void GetStatistics() {
            getStatisticsAttempts++;
            string url = "http://database.stijn.online/getStatistics.php?PHPSESSID=" + SessionID;
            using(var client = new HttpClient()) {
                client.Timeout = System.TimeSpan.FromSeconds(10);
                try {
                    var result = await client.GetAsync(url);
                    if(result.IsSuccessStatusCode) {
                        string content = await result.Content.ReadAsStringAsync();
                        if(content != "Please Login") {
                            DisplayStatistics(content);
                        } else {
                            ServerLogin();
                            if(getStatisticsAttempts < 5)
                                GetStatistics();
                        }
                    } else {
                        failed.text = "Request Failed";
                        StartCoroutine(resetFailedText());
                    }
                } catch(System.Exception e) {
                    failed.text = "Request Timed Out";
                    StartCoroutine(resetFailedText());
                }
            }
        }

        private async void ServerLogin() {
            string url = "http://database.stijn.online/serverLogin.php?server_name=" + databaseLoginUsername + "&password=" + databaseLoginPass;
            using(var client = new HttpClient()) {
                client.Timeout = System.TimeSpan.FromSeconds(10);
                try {
                    var result = await client.GetAsync(url);
                    if(result.IsSuccessStatusCode) {
                        string content = await result.Content.ReadAsStringAsync();
                        Debug.Log("SessionID: " + content);
                        if(content != "Incorrect Login Info") {
                            SessionID = content;
                        }
                    } else {
                        failed.text = "Request Failed";
                        StartCoroutine(resetFailedText());
                    }
                } catch(System.Exception e) {
                    failed.text = "Request Timed Out";
                    StartCoroutine(resetFailedText());
                }
            }
        }

        private void DisplayStatistics(string JSON) {
            Statistics statistics = JsonConvert.DeserializeObject<Statistics>(JSON);
            topWinners.text = "";
            foreach(Statistics.TopWinner item in statistics.topWinners) {
                topWinners.text += item.username + ": " + item.First + "\n";
            }
            topSecond.text = "";
            foreach(Statistics.TopSecond item in statistics.topSecond) {
                topSecond.text += item.username + ": " + item.Second + "\n";
            }
            monthlyPlays.text = "Plays this month: " + statistics.plays.ToString();
        }

        private IEnumerator resetFailedText() {
            yield return new WaitForSeconds(3f);
            failed.text = "";
        }


        //data structure similar to JSON output
        public class Statistics {
            public int plays;
            public TopWinner[] topWinners;
            public TopSecond[] topSecond;

            public class TopWinner {
                public string username;
                public int First;
            }

            public class TopSecond {
                public string username;
                public int Second;
            }
        }
    }

}

