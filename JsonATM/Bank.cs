using System.Text.Json;
using static JsonATM.BankHelper;

namespace JsonATM
{
    internal class Bank(string currency, string dataFilePath) : IDisposable
    {
        private List<Account> Accounts { get; set; } = GetAccountData(dataFilePath);
        public string Currency { get; set; } = currency;
        public string DataFilePath { get; set; } = dataFilePath;

        private static List<Account> GetAccountData(string dataFilePath)
        {
            string jsonData = File.ReadAllText(dataFilePath);

            return JsonSerializer.Deserialize<List<Account>>(jsonData) ?? [];
        }
        public void Dispose() { SaveAccountData(); }
        public void SaveAccountData()
        {
            string jsonData = JsonSerializer.Serialize(Accounts);
            File.WriteAllText(DataFilePath, jsonData);
        }

        public void Deposit(string accountNumber, double amount)
        {
            CheckNegativeAmount(amount);

            Account account = GetAccount(accountNumber);

            if (double.IsInfinity(account.Balance + amount)) {
                throw new ArgumentException("Invalid amount\n" +
                                            "Balance cannot be infinite");
            }

            account.Balance += amount;
        }
        public void Withdraw(string accountNumber, double amount)
        {
            CheckNegativeAmount(amount);

            Account account = GetAccount(accountNumber);

            if (account.Balance < amount) {
                throw new ArgumentException("Invalid amount\n" +
                                            "Withdrawal amount cannot exceed balance");
            }

            account.Balance -= amount;
        }
        public void NewAccount(string accountNumber)
        {
            if (!IsValidAccountNumber(accountNumber)) {
                throw new ArgumentException("Invalid account number\n" +
                                            "Format must be 'xxx-xxx', where x is a digit");
            }
            if (Accounts.Any(account => account.AccountNumber == accountNumber)) {
                throw new ArgumentException("Invalid account number\n" +
                                            "Account already exsists");
            }
            Accounts.Add(new Account(accountNumber));
        }
        public void DeleteAccount(string accountNumber)
        {
            Accounts.Remove(GetAccount(accountNumber));
        }
        public List<Account> GetAccounts()
        {
            return [.. Accounts]; // Return a copy of the list for security
        }
        public double GetBalance(string accountNumber)
        {
            return GetAccount(accountNumber).Balance;
        }
        private Account GetAccount(string accountNumber)
        {
            return Accounts.FirstOrDefault(account => account.AccountNumber == accountNumber)
                ?? throw new KeyNotFoundException("Account not found");
        }
    }
}