// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.

using FiiiCoin.DTO;
using FiiiCoin.Models;
using FiiiCoin.ServiceAgent;
using FiiiCoin.Utility;
using FiiiCoin.Utility.Api;
using System;
using System.Threading.Tasks;

namespace FiiiCoin.Business
{
    public static class BlockChainEngineApi
    {
        public static async Task<ApiResponse> GetBlockChainStatus()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                BlockChainStatus status = new BlockChainStatus();
                BlockChainStatusOM result = await engine.GetBlockChainStatus();
                if (result != null)
                {
                    status.ChainService = result.ChainService;
                    status.BlockService = result.BlockService;
                    status.P2pService = result.P2pService;
                    status.RpcService = result.RpcService;
                    status.ChainNetwork = result.ChainNetwork;
                    status.Height = result.Height;

                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(status);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        public static async Task<ApiResponse> StopEngine()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                await engine.StopEngine();
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        public static async Task<ApiResponse> GetBlockCount()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                long result = await engine.GetBlockCount();

                response.Result = Newtonsoft.Json.Linq.JToken.FromObject(result);
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        public static async Task<ApiResponse> GetBlockHash(long height)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                string result = await engine.GetBlockHash(height);
                if (!string.IsNullOrEmpty(result))
                {
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(result);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        public static async Task<ApiResponse> GetBlock(string hash, int formate = 0)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                BlockInfoOM result = await engine.GetBlock(hash, formate);
                if (result != null)
                {
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(result);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        public static async Task<ApiResponse> GetBlockHeader(string hash, int formate = 0)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                BlockHeaderOM result = await engine.GetBlockHeader(hash, formate);
                if (result != null)
                {
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(result);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        public static async Task<ApiResponse> GetChainTips()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                ChainTipsOM[] result = await engine.GetChainTips();
                if (result != null)
                {
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(result);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        public static async Task<ApiResponse> GetDifficulty()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                BlockDifficultyOM result = await engine.GetDifficulty();
                if (result != null)
                {
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(result);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        public static async Task<ApiResponse> GenerateNewBlock(string minerName, string address, int format = 0)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                BlockInfoOM result = await engine.GenerateNewBlock(minerName, address, format);
                if (result != null)
                {
                    response.Result = Newtonsoft.Json.Linq.JToken.FromObject(result);
                }
                else
                {
                    response.Result = null;
                }
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }

        public static async Task<ApiResponse> SubmitBlock(string blockData)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                BlockChainEngine engine = new BlockChainEngine();
                await engine.SubmitBlock(blockData);
            }
            catch (ApiCustomException ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Api error code is：" + ex.ErrorCode.ToString());
                Logger.Singleton.Error("Api error reason is：" + ex.InnerException.ToString());
                response.Error = new ApiError(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Singleton.Error(ex.Message);
                Logger.Singleton.Error("Custom error code is：" + ex.HResult);
                Logger.Singleton.Error("Custom error reason is：" + ex.InnerException);
                response.Error = new ApiError(ex.HResult, ex.Message);
            }
            return response;
        }
    }
}
