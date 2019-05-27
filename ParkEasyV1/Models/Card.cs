using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Subclass of Payment to hold all Card Payment details
    /// </summary>
    public class Card : Payment
    {
        /// <summary>
        /// Enum of the type of payment card
        /// </summary>
        [Display(Name = "Card Type")]
        public CardType Type { get; set; }

        /// <summary>
        /// Card number
        /// </summary>
        [DataType(DataType.CreditCard)]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        /// <summary>
        /// Name on payment card
        /// </summary>
        [Display(Name = "Name on Card")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string NameOnCard { get; set; }

        /// <summary>
        /// expiry date on card
        /// </summary>
        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM}", ApplyFormatInEditMode = true)]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Card security code
        /// </summary>
        public int CVV { get; set; }

        /// <summary>
        /// Function to encrypt the payment card details and return a hashed value to be stored safely in database
        /// </summary>
        /// <param name="clearText">the text to be encrypted</param>
        /// <returns>Hashed text</returns>
        private string Encrypt(string clearText)
        {
            try
            {
                string EncryptionKey = "MAKV2SPBNI99212";
                byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray());
                    }
                }
                return clearText;
            }
            catch (Exception ex)
            {
                //if exception occurs, return null
                return null;
            }
            
        }

        /// <summary>
        /// Function to decrypt the payment card details and return the clear text value retrieved from database
        /// </summary>
        /// <param name="cipherText">Encrpyted text</param>
        /// <returns>Clear text value</returns>
        private string Decrypt(string cipherText)
        {
            try
            {
                string EncryptionKey = "MAKV2SPBNI99212";
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
            catch (Exception ex)
            {
                //if exception occurs, return null
                return null;
            }
            

        }
    }
        /// <summary>
        /// Enumeration of the different types of ATM card accepted by the system
        /// </summary>
        public enum CardType
        {
            Visa,
            Mastercard,
            [Display(Name = "American Express")]
            AmericanExpress,
            Discover,
            Maestro
        }
    
}
