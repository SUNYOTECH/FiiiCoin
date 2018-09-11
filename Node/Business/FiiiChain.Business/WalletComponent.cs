// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or http://www.opensource.org/licenses/mit-license.php.
using FiiiChain.Data;
using FiiiChain.Entities;
using FiiiChain.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FiiiChain.Business
{
    public class WalletComponent
    {
        const string encryptExtensionName = ".fcdatx";
        const string noEncryptExtensionName = ".fcdat";

        public void BackupWallet(string filePath, string salt)
        {
            string extensionName = Path.GetExtension(filePath).ToLower();
            if (string.IsNullOrWhiteSpace(salt))
            {
                filePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + noEncryptExtensionName);
            }
            else
            {
                filePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + encryptExtensionName);
            }

            WalletBackup backup = null;
            try
            {
                backup = new WalletBackup()
                {
                    AccountList = new AccountDac().SelectAll(),
                    AddressBookItemList = new AddressBookDac().SelectAll(),
                    SettingList = new SettingDac().SelectAll(),
                    TransactionCommentList = new TransactionCommentDac().SelectAll()
                };
            }
            catch (Exception ex)
            {
                throw new CommonException(ErrorCode.Engine.Wallet.DB.LOAD_DATA_ERROR, ex);
            }

            try
            {
                if (backup != null)
                {
                    if (extensionName == noEncryptExtensionName)
                    {
                        SaveFile(backup, filePath);
                    }
                    else
                    {
                        SaveFile(backup, filePath, salt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CommonException(ErrorCode.Engine.Wallet.DATA_SAVE_TO_FILE_ERROR, ex);
            }
        }

        public bool RestoreWalletBackup(string filePath, string salt)
        {
            bool result = false;

            string extensionName = Path.GetExtension(filePath).ToLower();
            if (extensionName != encryptExtensionName && extensionName != noEncryptExtensionName) { throw new CommonException(ErrorCode.Engine.Wallet.IO.EXTENSION_NAME_NOT_SUPPORT); }

            //Load backup file data.
            WalletBackup backup = null;
            if (extensionName == noEncryptExtensionName)
            {
                backup = LoadFile<WalletBackup>(filePath);
            }
            else
            {
                backup = LoadFile<WalletBackup>(filePath, salt);
            }


            //Create SQL Query Script and run
            try
            {
                if (backup != null)
                {
                    WalletBackupDac dac = new WalletBackupDac();
                    int count = -1;
                    count = dac.Restore(backup);
                    if (count > -1)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CommonException(ErrorCode.Engine.Wallet.DB.EXECUTE_SQL_ERROR, ex);
            }

            return result;
        }

        public bool EncryptWallet(string salt)
        {
            bool result = false;
            AccountDac adac = new AccountDac();
            var aclist = adac.SelectAll();
            foreach (var item in aclist)
            {
                item.PrivateKey = AES128.Encrypt(item.PrivateKey, salt);
            }
            try
            {
                var rows = adac.UpdatePrivateKeyAr(aclist);
                if (rows > -1)
                {
                    SettingComponent sComponent = new SettingComponent();
                    var setting = sComponent.GetSetting();
                    setting.Encrypt = true;
                    setting.PassCiphertext = MD5Helper.EncryptTo32(salt);
                    sComponent.SaveSetting(setting);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw new CommonException(ErrorCode.Engine.Wallet.DB.EXECUTE_SQL_ERROR, ex);
            }
            return result;
        }

        public bool CheckPassword(string password)
        {
            SettingComponent sComponent = new SettingComponent();
            var setting = sComponent.GetSetting();
            if (MD5Helper.EncryptTo32(password) == setting.PassCiphertext)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ChangePassword(string oldPassword, string newPassword)
        {
            SettingComponent sComponent = new SettingComponent();
            var setting = sComponent.GetSetting();
            if (MD5Helper.EncryptTo32(oldPassword) == setting.PassCiphertext)
            {
                AccountDac adac = new AccountDac();
                var aclist = adac.SelectAll();
                foreach (var item in aclist)
                {
                    item.PrivateKey = AES128.Decrypt(item.PrivateKey, oldPassword);
                    item.PrivateKey = AES128.Encrypt(item.PrivateKey, newPassword);
                }
                try
                {
                    var rows = adac.UpdatePrivateKeyAr(aclist);
                    if (rows > -1)
                    {
                        setting = sComponent.GetSetting();
                        setting.Encrypt = true;
                        setting.PassCiphertext = MD5Helper.EncryptTo32(newPassword);
                        sComponent.SaveSetting(setting);
                    }
                }
                catch (Exception ex)
                {
                    throw new CommonException(ErrorCode.Engine.Wallet.DB.EXECUTE_SQL_ERROR, ex);
                }
            }
            else
            {
                throw new CommonException(ErrorCode.Engine.Wallet.CHECK_PASSWORD_ERROR);
            }
        }

        #region IO oprerations
        /// <summary>
        /// Object use json formatting, and save to file system
        /// </summary>
        /// <typeparam name="T">any type</typeparam>
        /// <param name="obj">any type entity</param>
        /// <param name="filePath">saved path</param>
        private static void SaveFile<T>(T obj, string filePath)
        {
            string jsonString = JsonConvert.SerializeObject(obj);
            FileHelper.StringSaveFile(jsonString, filePath);
        }

        /// <summary>
        /// [encrypt]Object use json formatting, and save to file system
        /// </summary>
        /// <typeparam name="T">any type</typeparam>
        /// <param name="obj">any type entity</param>
        /// <param name="filePath">saved path</param>
        /// <param name="salt">encrypt salt</param>
        private static void SaveFile<T>(T obj, string filePath, string salt)
        {
            string jsonString = JsonConvert.SerializeObject(obj);
            string encryptString = jsonString;

            if (!string.IsNullOrEmpty(salt))
            {
                encryptString = AES128.Encrypt(jsonString, salt);
            }
            FileHelper.StringSaveFile(encryptString, filePath);
        }


        /// <summary>
        /// Read Object from File
        /// </summary>
        /// <param name="filePath">file location path</param>
        /// <returns>Load entity.</returns>
        private static T LoadFile<T>(string filePath)
        {
            if (!File.Exists(filePath)) { throw new CommonException(ErrorCode.Engine.Wallet.IO.FILE_NOT_FOUND); }
            string fileString = string.Empty;
            try
            {
                fileString = FileHelper.LoadFileString(filePath);
            }
            catch (Exception ex)
            {
                throw new CommonException(ErrorCode.Engine.Wallet.IO.FILE_DATA_INVALID, ex);
            }
            return JsonConvert.DeserializeObject<T>(fileString);
        }

        /// <summary>
        /// Read Object from File
        /// </summary>
        /// <param name="filePath">file location path</param>
        /// <param name="salt"></param>
        /// <returns>dencrypt entity.</returns>
        private static T LoadFile<T>(string filePath, string salt)
        {
            if (!File.Exists(filePath)) { throw new CommonException(ErrorCode.Engine.Wallet.IO.FILE_NOT_FOUND); }
            string fileString = string.Empty;
            try
            {
                fileString = FileHelper.LoadFileString(filePath);
            }
            catch (Exception ex)
            {
                throw new CommonException(ErrorCode.Engine.Wallet.IO.FILE_DATA_INVALID, ex);
            }

            try
            {
                fileString = AES128.Decrypt(fileString, salt);
            }
            catch (Exception ex)
            {
                throw new CommonException(ErrorCode.Engine.Wallet.DECRYPT_DATA_ERROR, ex);
            }
            return JsonConvert.DeserializeObject<T>(fileString);
        }
        #endregion
    }
}
