﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WordPressPCL.Interfaces;
using WordPressPCL.Utility;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using WordPressPCL.Models;
using System.IO;

namespace WordPressPCL.Client
{
    /// <summary>
    /// Client class for interaction with Media endpoint WP REST API
    /// </summary>
    public class Media : ICRUDOperationAsync<MediaItem>
    {
        #region Init
        private string _defaultPath;
        private const string _methodPath = "media";
        private HttpHelper _httpHelper;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="HttpHelper">reference to HttpHelper class for interaction with HTTP</param>
        /// <param name="defaultPath">path to site, EX. http://demo.com/wp-json/ </param>
        public Media(ref HttpHelper HttpHelper, string defaultPath)
        {
            _defaultPath = defaultPath;
            _httpHelper = HttpHelper;
        }
        #endregion
        /// <summary>
        /// Create a parametrized query and get a result
        /// </summary>
        /// <param name="queryBuilder">Media query builder with specific parameters</param>
        /// <param name="useAuth">Send request with authenication header</param>
        /// <returns>List of filtered Media</returns>
        public async Task<IEnumerable<Media>> Query(MediaQueryBuilder queryBuilder, bool useAuth = false)
        {
            return await _httpHelper.GetRequest<IEnumerable<Media>>($"{_defaultPath}{_methodPath}{queryBuilder.BuildQueryURL()}", false,useAuth).ConfigureAwait(false);
        }
        #region Interface Realisation
        /// <summary>
        /// Create Empty Media
        /// </summary>
        /// <param name="Entity">Media object</param>
        /// <returns>Created media object</returns>
        public async Task<MediaItem> Create(MediaItem Entity)
        {
            throw new InvalidOperationException("Use Create(fileStream,fileName) instead of this method");
            /*var postBody = new StringContent(JsonConvert.SerializeObject(Entity).ToString(), Encoding.UTF8, "application/json");
            return (await _httpHelper.PostRequest<MediaItem>($"{_defaultPath}{_methodPath}", postBody)).Item1;*/
        }
        /// <summary>
        /// Create Media entity with attachment
        /// </summary>
        /// <param name="fileStream">stream with file content</param>
        /// <param name="filename">Name of file in WP Media Library</param>
        /// <returns>Created media object</returns>
        public async Task<MediaItem> Create(Stream fileStream,string filename)
        {
            StreamContent content = new StreamContent(fileStream);
            string extension = filename.Split('.').Last();
            content.Headers.TryAddWithoutValidation("Content-Type", MimeTypeHelper.GetMIMETypeFromExtension(extension));
            content.Headers.TryAddWithoutValidation("Content-Disposition", $"attachment; filename={filename}");
            return (await _httpHelper.PostRequest<MediaItem>($"{_defaultPath}{_methodPath}", content)).Item1;
        }
        /// <summary>
        /// Update Media
        /// </summary>
        /// <param name="Entity">Media object</param>
        /// <returns>Updated media object</returns>
        public async Task<MediaItem> Update(MediaItem Entity)
        {
            var postBody = new StringContent(JsonConvert.SerializeObject(Entity).ToString(), Encoding.UTF8, "application/json");
            return (await _httpHelper.PostRequest<MediaItem>($"{_defaultPath}{_methodPath}/{Entity.Id}", postBody)).Item1;
        }
        /// <summary>
        /// Delete Media
        /// </summary>
        /// <param name="ID">Media Id</param>
        /// <returns>Result of operation</returns>
        public async Task<HttpResponseMessage> Delete(int ID)
        {
            return await _httpHelper.DeleteRequest($"{_defaultPath}{_methodPath}/{ID}?force=true").ConfigureAwait(false);
        }
        /// <summary>
        /// Get All Media
        /// </summary>
        /// <param name="embed">Include embed info</param>
        /// <param name="useAuth">Send request with authenication header</param>
        /// <returns>List of all Media</returns>
        public async Task<IEnumerable<MediaItem>> GetAll(bool embed = false, bool useAuth = false)
        {
            //100 - Max posts per page in WordPress REST API, so this is hack with multiple requests
            var medias = new List<MediaItem>();
            var medias_page = new List<MediaItem>();
            int page = 1;
            do
            {
                medias_page = (await _httpHelper.GetRequest<IEnumerable<MediaItem>>($"{_defaultPath}{_methodPath}?per_page=100&page={page++}", embed,useAuth).ConfigureAwait(false))?.ToList<MediaItem>();
                if (medias_page != null && medias_page.Count > 0) { medias.AddRange(medias_page); }

            } while (medias_page != null && medias_page.Count > 0);

            return medias;
            //return await _httpHelper.GetRequest<IEnumerable<Media>>($"{_defaultPath}{_methodPath}", embed).ConfigureAwait(false);
        }
        /// <summary>
        /// Get Media by Id
        /// </summary>
        /// <param name="ID">Media ID</param>
        /// <param name="embed">include embed info</param>
        /// <param name="useAuth">Send request with authenication header</param>
        /// <returns>Media by Id</returns>
        public async Task<MediaItem> GetByID(int ID, bool embed = false, bool useAuth = false)
        {
            return await _httpHelper.GetRequest<MediaItem>($"{_defaultPath}{_methodPath}/{ID}", embed,useAuth).ConfigureAwait(false);
        }
        /// <summary>
        /// Force deletion of media
        /// </summary>
        /// <param name="ID">Media id</param>
        /// <param name="force">force deletion</param>
        /// <returns>Result of operation</returns>
        public async Task<HttpResponseMessage> Delete(int ID,bool force=false)
        {
            return await _httpHelper.DeleteRequest($"{_defaultPath}{_methodPath}/{ID}?force={force}").ConfigureAwait(false);
        }
        #endregion
    }
}
