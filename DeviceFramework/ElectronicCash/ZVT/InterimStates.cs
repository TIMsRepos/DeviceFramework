namespace TIM.Devices.Framework.ElectronicCash.ZVT
{
    public static class InterimStates
    {
        public static string GetMessage(byte bytState)
        {
            if (bytState == 0) return "BZT wartet auf Betragbestätigung";
            else if (bytState == 1) return "Bitte Anzeigen auf dem PIN-Pad beachten";
            else if (bytState == 2) return "Bitte Anzeigen auf dem PIN-Pad beachten";
            else if (bytState == 3) return "Vorgang nicht möglich";
            else if (bytState == 4) return "BZT wartet auf Antwort vom FEP";
            else if (bytState == 5) return "BZT sendet Autostorno";
            else if (bytState == 6) return "BZT sendet Nachbuchungen";
            else if (bytState == 7) return "Karte nicht zugelassen";
            else if (bytState == 8) return "Karte unbekannt / undefiniert";
            else if (bytState == 9) return "Karte verfallen";
            else if (bytState == 10) return "Karte einstecken";
            else if (bytState == 11) return "Bitte Karte entnehmen !";
            else if (bytState == 12) return "Karte nicht lesbar";
            else if (bytState == 13) return "Vorgang abgebrochen";
            else if (bytState == 14) return "Vorgang wird bearbeitet bitte warten...";
            else if (bytState == 15) return "BZT leitet einen automatischen Kassenabschluß ein";
            else if (bytState == 16) return "Karte ungültig";
            else if (bytState == 17) return "Guthabenanzeige";
            else if (bytState == 18) return "Systemfehler";
            else if (bytState == 19) return "Zahlung nicht möglich";
            else if (bytState == 20) return "Guthaben nicht ausreichend";
            else if (bytState == 21) return "Geheimzahl falsch";
            else if (bytState == 22) return "Limit nicht ausreichend";
            else if (bytState == 23) return "Bitte warten...";
            else if (bytState == 24) return "Geheimzahl zu oft falsch";
            else if (bytState == 25) return "Kartendaten falsch";
            else if (bytState == 26) return "Servicemodus";
            else if (bytState == 27) return "Zahlung erfolgt. Bitte tanken";
            else if (bytState == 28) return "Zahlung erfolgt. Bitte Ware entnehmen";
            else if (bytState == 29) return "Autorisierung nicht möglich";
            else if (bytState == 38) return "BZT wartet auf Eingabe der Mobilfunknummer";
            else if (bytState == 39) return "BZT wartet auf Wiederholung der Mobilfunknummer";
            else if (bytState == 65) return "Bitte Anzeigen auf dem PIN-Pad beachten Bitte Karte entnehmen !";
            else if (bytState == 66) return "Bitte Anzeigen auf dem PIN-Pad beachten Bitte Karte entnehmen !";
            else if (bytState == 67) return "Vorgang nicht möglich Bitte Karte entnehmen !";
            else if (bytState == 68) return "BZT wartet auf Antwort vom FEP Bitte Karte entnehmen !";
            else if (bytState == 69) return "BZT sendet Autostorno Bitte Karte entnehmen !";
            else if (bytState == 70) return "BZT sendet Nachbuchungen Bitte Karte entnehmen !";
            else if (bytState == 71) return "Karte nicht zugelassen Bitte Karte entnehmen !";
            else if (bytState == 72) return "Karte unbekannt / undefiniert Bitte Karte entnehmen !";
            else if (bytState == 73) return "Karte verfallen Bitte Karte entnehmen !";
            else if (bytState == 74) return "Karte einstecken Bitte Karte entnehmen !";
            else if (bytState == 75) return "Bitte Karte entnehmen !";
            else if (bytState == 76) return "Karte nicht lesbar Bitte Karte entnehmen !";
            else if (bytState == 77) return "Vorgang abgebrochen Bitte Karte entnehmen !";
            else if (bytState == 78) return "Vorgang wird bearbeitet bitte warten... Bitte Karte entnehmen !";
            else if (bytState == 79) return "BZT leitet einen automatischen Kassenabschluß ein Bitte Karte entnehmen !";
            else if (bytState == 80) return "Karte ungültig Bitte Karte entnehmen !";
            else if (bytState == 81) return "Guthabenanzeige Bitte Karte entnehmen !";
            else if (bytState == 82) return "Systemfehler Bitte Karte entnehmen !";
            else if (bytState == 83) return "Zahlung nicht möglich Bitte Karte entnehmen !";
            else if (bytState == 84) return "Guthaben nicht ausreichend Bitte Karte entnehmen !";
            else if (bytState == 85) return "Geheimzahl falsch Bitte Karte entnehmen !";
            else if (bytState == 86) return "Limit nicht ausreichend Bitte Karte entnehmen !";
            else if (bytState == 87) return "Bitte warten... Bitte Karte entnehmen !";
            else if (bytState == 88) return "Geheimzahl zu oft falsch Bitte Karte entnehmen !";
            else if (bytState == 89) return "Kartendaten falsch Bitte Karte entnehmen !";
            else if (bytState == 90) return "Servicemodus Bitte Karte entnehmen !";
            else if (bytState == 91) return "Zahlung erfolgt. Bitte tanken Bitte Karte entnehmen !";
            else if (bytState == 92) return "Zahlung erfolgt. Bitte Ware entnehmen Bitte Karte entnehmen !";
            else if (bytState == 93) return "Autorisierung nicht möglich Bitte Karte entnehmen !";
            else if (bytState == 102) return "BZT wartet auf Eingabe der Mobilfunknummer Bitte Karte entnehmen !";
            else if (bytState == 103) return "BZT wartet auf Wiederholung der Mobilfunknummer Bitte Karte entnehmen !";
            else if (bytState == 199) return "BZT wartet auf Eingabe des Kilometerstands";
            else if (bytState == 200) return "BZT wartet auf Kassierer";
            else if (bytState == 201) return "BZT leitet eine automatische Diagnose ein";
            else if (bytState == 202) return "BZT leitet eine automatische Initialisierung ein";
            else if (bytState == 210) return "DFÜ Verbindung wird hergestellt";
            else if (bytState == 211) return "DFÜ Verbindung besteht";
            else if (bytState == 224) return "BZT wartet auf Anwendungsauswahl";
            else if (bytState == 225) return "BZT wartet auf Sprachauswahl";
            else if (bytState == 241) return "offline";
            else if (bytState == 242) return "online";
            else if (bytState == 243) return "offline Transaktion";
            else return "UNKNOWN STATE";
        }
    }
}