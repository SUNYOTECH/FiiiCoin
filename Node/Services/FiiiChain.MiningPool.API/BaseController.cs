using FiiiChain.Consensus.Api;
using FiiiChain.Framework;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace FiiiChain.MiningPool.API
{
    public class BaseController : Controller
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public CommonResponse Error(int errorCode, string message, Exception exception = null)
        {
            if (errorCode != ErrorCode.UNKNOWN_ERROR)
            {
                LogHelper.Error(message);
            }
            else
            {
                LogHelper.Fatal(message, exception);
            }

            return new CommonResponse { Data = null, Code = 1, Extension = "", Message = message };
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public CommonResponse OK(object obj = null)
        {
            if (obj == null)
            {
                return new CommonResponse { Data = null, Code = 0, Extension = "", Message = "successful" };
            }
            return new CommonResponse { Data = Newtonsoft.Json.Linq.JToken.FromObject(obj), Code = 0, Extension = "", Message = "successful" };
        }
    }
}
