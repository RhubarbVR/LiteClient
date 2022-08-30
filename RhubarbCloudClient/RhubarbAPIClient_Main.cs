﻿using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RhubarbCloudClient.Model;

namespace RhubarbCloudClient
{
	public partial class RhubarbAPIClient : IDisposable
	{
		public const string API_PATH = "api/";

		public string FileSaveLocation;
		public Action<CookieContainer> OnWrite;
		private HttpClientHandler HttpClientHandler { get; set; }
		public CookieContainer Cookies => HttpClientHandler?.CookieContainer;
		public HttpClient HttpClient;

		public RhubarbAPIClient(Uri baseAdress, Func<CookieContainer> onRead, Action<CookieContainer> onWrite) {
			HttpClientHandler = new HttpClientHandler {
				AllowAutoRedirect = true,
				UseCookies = true,
				CookieContainer = onRead()
			};
			HttpClient = new HttpClient(HttpClientHandler) {
				BaseAddress = baseAdress
			};
			OnWrite = onWrite;
			UpdateCheckForInternetConnection();
		}

		public RhubarbAPIClient(HttpClient httpClient) {
			HttpClient = httpClient;
			UpdateCheckForInternetConnection();
		}
		public RhubarbAPIClient(Uri baseAdress, string fileName = null) {
			if (fileName is not null) {
				HttpClientHandler = new HttpClientHandler {
					AllowAutoRedirect = true,
					UseCookies = true,
					CookieContainer = ReadCookiesFromDisk(fileName)
				};
				HttpClient = new HttpClient(HttpClientHandler) {
					BaseAddress = baseAdress
				};
				FileSaveLocation = fileName;
			}
			else {
				HttpClient = new HttpClient() {
					BaseAddress = baseAdress
				};
			}
			UpdateCheckForInternetConnection();
		}

		public void Dispose() {
			if (FileSaveLocation is not null) {
				WriteCookiesToDisk(FileSaveLocation, Cookies);
			}
			else {
				OnWrite?.Invoke(Cookies);
			}
		}
	}
}