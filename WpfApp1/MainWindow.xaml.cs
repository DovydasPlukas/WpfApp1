using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        string raktas = "slaptas123";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Mygtukas_uzkodavimas_Click(object sender, RoutedEventArgs e)
        {
            string tekstas = Uzkodavimo_tekstas.Text;

            if (string.IsNullOrWhiteSpace(tekstas))
            {
                MessageBox.Show("Įveskite tekstą.");
                return;
            }
            if (AES_radio.IsChecked == true)
            {
                try
                {
                    string uzkoduotasAES = UzkoduotiAES(tekstas, Encoding.UTF8.GetBytes(raktas.PadRight(32, '\0')));
                    Uzkoduotas.Text = uzkoduotasAES;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Klaida užkodavimo metu: {ex.Message}");
                }
            }
            else if (TripleDES_radio.IsChecked == true)
            {
                try
                {
                    string uzkoduotas3DES = Uzkoduoti3DES(tekstas, Encoding.UTF8.GetBytes(raktas.PadRight(24, '\0')));
                    Uzkoduotas.Text = uzkoduotas3DES;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Klaida užkodavimo metu: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite užkodavimo metodą (AES arba TripleDES).");
            }
        }

        static string UzkoduotiAES(string informacija, byte[] raktas)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = raktas;
                aesAlg.IV = new byte[16];

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(informacija);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        static string Uzkoduoti3DES(string informacija, byte[] raktas)
        {
            using (TripleDESCryptoServiceProvider tdesAlg = new TripleDESCryptoServiceProvider())
            {
                tdesAlg.Key = raktas;
                tdesAlg.IV = new byte[8];
                tdesAlg.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = tdesAlg.CreateEncryptor(tdesAlg.Key, tdesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(informacija);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private void Uzkodavimo_tekstas_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
        private void Uzkuodomas_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
        private void Mygtukas_atkodavimas_Click(object sender, RoutedEventArgs e)
        {
            string tekstas = Atkodavimo_tekstas.Text;

            if (string.IsNullOrWhiteSpace(tekstas))
            {
                MessageBox.Show("Įveskite tekstą.");
                return;
            }
            if (AES_radio.IsChecked == true)
            {
                try
                {
                    string dekoduotasAES = DekoduotiAES(tekstas, raktas);
                    atkoduotas.Text = dekoduotasAES;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Klaida atkodavimo metu: {ex.Message}");
                }
            }
            else if (TripleDES_radio.IsChecked == true)
            {
                try
                {
                    byte[] raktasBytes = Encoding.ASCII.GetBytes(raktas.PadRight(24, '\0'));
                    string dekoduotas3DES = Dekoduoti3DES(tekstas, Encoding.ASCII.GetString(raktasBytes));
                    atkoduotas.Text = dekoduotas3DES;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Klaida atkodavimo metu: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Pasirinkite atkodavimo metodą (AES arba TripleDES).");
            }
        }


        static string DekoduotiAES(string uzkoduotasTekstas, string slaptazodis)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(slaptazodis.PadRight(32, '\0')).Take(32).ToArray();
                    aesAlg.IV = new byte[16];

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(uzkoduotasTekstas)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd().Trim();
                            }
                        }
                    }
                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Dekodavimo klaida: {ex.Message}");
                return null;
            }
        }

        static string Dekoduoti3DES(string uzkoduotasTekstas, string slaptazodis)
        {
            try
            {
                using (TripleDESCryptoServiceProvider tdesAlg = new TripleDESCryptoServiceProvider())
                {
                    byte[] keyBytes = Encoding.UTF8.GetBytes(slaptazodis);

                    tdesAlg.Key = keyBytes.Take(24).ToArray();
                    tdesAlg.IV = new byte[8];
                    tdesAlg.Mode = CipherMode.CBC;

                    ICryptoTransform decryptor = tdesAlg.CreateDecryptor(tdesAlg.Key, tdesAlg.IV);

                    byte[] encryptedBytes = Convert.FromBase64String(uzkoduotasTekstas);

                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                    return Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Dekodavimo klaida: {ex.Message}");
                return null;
            }
        }
    }
}