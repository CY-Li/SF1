using DomainEntityDTO.Common;
using System.Net.Http.Headers;

namespace DotNetBackEndApi.Shared
{
    public class HttpClientService
    {
        private readonly HttpClient _HttpClient;
        private object? processQueryResult;

        public HttpClientService(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }

        public ApiResultModel<bool> Process(string apiName, object data)
        {
            //StartProcess(apiName, data).GetAwaiter().GetResult();
            StartProcessQuery<bool>(apiName, data).GetAwaiter().GetResult();
            return (ApiResultModel<bool>)processQueryResult;
        }

        public ApiResultModel<T> ProcessQuery<T>(string apiName, object data)
        {
            StartProcessQuery<T>(apiName, data).GetAwaiter().GetResult();

            return (ApiResultModel<T>)processQueryResult;
        }

        private async Task StartProcessQuery<T>(string apiName, object data)
        {
            _HttpClient.DefaultRequestHeaders.Accept.Clear();
            _HttpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage? response = null;
            try
            {
                response = await _HttpClient.PostAsJsonAsync(
                    $"api/" + apiName + "", data);

                response.EnsureSuccessStatusCode();

                // Deserialize the updated product from the response body.
                processQueryResult = await response.Content.ReadFromJsonAsync<ApiResultModel<T>>();
            }
            catch (Exception ex)
            {
                if (response == null)
                {
                    processQueryResult = new ApiResultModel<T>()
                    {
                        returnStatus = 999,
                        returnMsg = ex.Message
                    };
                }
                else if (await response.Content.ReadAsStringAsync() != String.Empty)
                {
                    var m_errors = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, object>>(await response.Content.ReadAsStringAsync());

                    if (m_errors != null && m_errors.Any(a => a.Key == "errors") && m_errors["errors"] != null)
                    {
                        processQueryResult = new ApiResultModel<T>()
                        {
                            returnStatus = 999,
                            returnMsg = m_errors["errors"].ToString()
                        };
                        //throw new Exception(m_errors["errors"].ToString());
                    }
                    else if (m_errors != null && m_errors.Any(a => a.Key == "errorMsg"))
                    {
                        processQueryResult = new ApiResultModel<T>()
                        {
                            returnStatus = 999,
                            returnMsg = m_errors["errorMsg"].ToString()
                        };
                        //throw new Exception(m_errors["errorMsg"].ToString());
                    }
                    else
                    {
                        processQueryResult = new ApiResultModel<T>()
                        {
                            returnStatus = 999,
                            returnMsg = ex.Message
                        };
                        //throw;
                    }
                }
                else
                {
                    processQueryResult = new ApiResultModel<T>()
                    {
                        returnStatus = 999,
                        returnMsg = ex.Message
                    };
                }

            }
        }
    }
}
