namespace TIM.Devices.Framework.ElectronicCash.ZVT
{
    public static class ErrorMessages
    {
        public static string GetMessage(byte bytErrorCode)
        {
            if (bytErrorCode == 00) return "no error";
            else if (bytErrorCode >= 01 && bytErrorCode <= 99) return "errorcodes from network-operator system/authorisation-system";
            else if (bytErrorCode == 100) return "card not readable (LRC-/parity-error)";
            else if (bytErrorCode == 101) return "card-data not present (neither track-data nor chip found)";
            else if (bytErrorCode == 102) return "processing-error (also for problems with card-reader mechanism)";
            else if (bytErrorCode == 103) return "function not permitted for ec- and Maestro-cards";
            else if (bytErrorCode == 104) return "function not permitted for credit- and tank-cards";
            else if (bytErrorCode == 106) return "turnover-file full";
            else if (bytErrorCode == 107) return "function deactivated (PT not registered)";
            else if (bytErrorCode == 108) return "abort via time-out or abort-key";
            else if (bytErrorCode == 110) return "card in blocked-list (response to command 06 E4)";
            else if (bytErrorCode == 111) return "wrong currency";
            else if (bytErrorCode == 113) return "credit not sufficient (chip-card)";
            else if (bytErrorCode == 114) return "chip error";
            else if (bytErrorCode == 115) return "card-data incorrect (e.g. country-key check, checksum-error)";
            else if (bytErrorCode == 119) return "end-of-day batch not possible";
            else if (bytErrorCode == 120) return "card expired";
            else if (bytErrorCode == 121) return "card not yet valid";
            else if (bytErrorCode == 122) return "card unknown";
            else if (bytErrorCode == 125) return "communication error (communication module does not answer or is not present)";
            else if (bytErrorCode == 131) return "function not possible";
            else if (bytErrorCode == 133) return "key missing";
            else if (bytErrorCode == 137) return "PIN-pad defective";
            else if (bytErrorCode == 154) return "protocol error";
            else if (bytErrorCode == 155) return "error from dial-up/communication fault";
            else if (bytErrorCode == 156) return "please wait";
            else if (bytErrorCode == 160) return "receiver not ready";
            else if (bytErrorCode == 161) return "remote station does not respond";
            else if (bytErrorCode == 163) return "no connection";
            else if (bytErrorCode == 164) return "submission of Geldkarte not possible";
            else if (bytErrorCode == 177) return "memory full";
            else if (bytErrorCode == 178) return "merchant-journal full";
            else if (bytErrorCode == 180) return "already reversed";
            else if (bytErrorCode == 181) return "reversal not possible";
            else if (bytErrorCode == 183) return "pre-authorisation incorrect (amount too high) or amount wrong";
            else if (bytErrorCode == 184) return "error pre-authorisation";
            else if (bytErrorCode == 191) return "voltage supply to low (external power supply)";
            else if (bytErrorCode == 192) return "card locking mechanism defective";
            else if (bytErrorCode == 193) return "merchant-card locked";
            else if (bytErrorCode == 194) return "diagnosis required";
            else if (bytErrorCode == 195) return "maximum amount exceeded";
            else if (bytErrorCode == 196) return "card-profile invalid. New card-profiles must be loaded.";
            else if (bytErrorCode == 197) return "payment method not supported";
            else if (bytErrorCode == 198) return "currency not applicable";
            else if (bytErrorCode == 200) return "amount zu small";
            else if (bytErrorCode == 201) return "max. transaction-amount zu small";
            else if (bytErrorCode == 203) return "function only allowed in EURO";
            else if (bytErrorCode == 204) return "printer not ready";
            else if (bytErrorCode == 210) return "function not permitted for service-cards/bank-customer-cards";
            else if (bytErrorCode == 220) return "card inserted";
            else if (bytErrorCode == 221) return "error during card-eject (for motor-insertion reader)";
            else if (bytErrorCode == 222) return "error during card-insertion (for motor-insertion reader)";
            else if (bytErrorCode == 224) return "remote-maintenance activated";
            else if (bytErrorCode == 226) return "card-reader does not answer / card-reader defective";
            else if (bytErrorCode == 227) return "shutter closed";
            else if (bytErrorCode == 231) return "min. one goods-group not found";
            else if (bytErrorCode == 232) return "no  goods-groups-table loaded";
            else if (bytErrorCode == 233) return "restriction-code not permitted";
            else if (bytErrorCode == 234) return "card-code not permitted (e.g. card not activated via Diagnosis)";
            else if (bytErrorCode == 235) return "function not executable (PIN-algorithm unknown)";
            else if (bytErrorCode == 236) return "PIN-processing not possible";
            else if (bytErrorCode == 237) return "PIN-pad defective";
            else if (bytErrorCode == 240) return "open end-of-day batch present";
            else if (bytErrorCode == 241) return "ec-cash/Maestro offline error";
            else if (bytErrorCode == 245) return "OPT-error";
            else if (bytErrorCode == 246) return "OPT-data not available (= OPT personalisation required)";
            else if (bytErrorCode == 250) return "error transmitting offline-transactions (clearing error)";
            else if (bytErrorCode == 251) return "turnover data-set defective";
            else if (bytErrorCode == 252) return "necessary device not present or defective";
            else if (bytErrorCode == 253) return "baudrate not supported";
            else if (bytErrorCode == 254) return "register unknown";
            else if (bytErrorCode == 255) return "system error (= other/unknown error), See TLV tags 1F16 and 1F17";
            else return "UNKNOWN ERROR";
        }
    }
}