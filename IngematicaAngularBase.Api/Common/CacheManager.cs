using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Configuration;
using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using IngematicaAngularBase.Model.ViewModels;
using IngematicaAngularBase.Bll;

namespace IngematicaAngularBase.Api.Common
{

    public class SecurityManager
    {
        public static SecurityUsuarioViewModel GetSecurityForUser(string user)
        {

            SecurityUsuarioViewModel result;
           
            result = (SecurityUsuarioViewModel)CacheManager.Save(user + ".UserData",
                    () =>
                    {
                        SecurityBusiness bs = new SecurityBusiness();
                        return bs.GetSecurityUser(user);
                    }
                    , (6 * 3600)
                    , "Security");

            result.Reglas = GetRolReglas(result.IdRol);

            return result;
        }

        private static List<string> GetRolReglas(int idRol)
        {

            List<string> result;
            string key = idRol.ToString() + ".RolData";
            result = (List<string>)CacheManager.Save(key,
                    () =>
                    {
                        SecurityBusiness bs = new SecurityBusiness();
                        return bs.GetyRolReglas(idRol);
                    }
                    , (6 * 3600)
                    , "Security");

            return result;
        }

        public static int GetIdUsuario(string user)
        {
            return GetSecurityForUser(user).IdUsuario;
        }

        public static void InvalidateUser(string user)
        {

            CacheManager.Delete(user + ".UserData");
        }

        public static void InvalidateRolByUser(string user)
        {
            
            CacheManager.Delete(GetSecurityForUser(user).IdRol.ToString() + ".RolData");
        }

        public static void InvalidateRol(int idRol)
        {

            CacheManager.Delete(idRol.ToString() + ".RolData");
        }

        public static void ClearUserSecurityData()
        {
           CacheManager.DeleteByTag("Security");
        }

    }



    public class CacheManager
    {
        [Serializable]
        private class TagTreeItem
        {
            public string Tag;
            private ConcurrentDictionary<string, TagTreeItem> _Childs;
            public ConcurrentDictionary<string, TagTreeItem> Childs
            {
                get { return _Childs; }
            }
            private ConcurrentDictionary<string, string> _Keys;

            public ConcurrentDictionary<string, string> Keys
            {
                get { return _Keys; }
            }

            public TagTreeItem(string tag)
            {
                Tag = tag;
                _Childs = new ConcurrentDictionary<string, TagTreeItem>();
                _Keys = new ConcurrentDictionary<string, string>();
            }

            public void Clear()
            {
                _Childs.Clear();
                _Keys.Clear();
            }

            private void Add(string key, int idx = 0, params string[] tags)
            {
                var item = _Childs.GetOrAdd(tags[idx], (id) => { return new TagTreeItem(id); });
                if (idx == tags.Length - 1)
                {
                    item.Keys.GetOrAdd(key, key);
                }
                else
                {
                    item.Add(key, idx + 1, tags);
                }
            }

            public void Add(string key, params string[] tags)
            {
                if (tags != null && tags.Length > 0)
                {
                    Add(key, 0, tags);
                }
                else
                {
                    _Keys.GetOrAdd(key, key);
                }
            }

            public bool RemoveKey(string key)
            {
                if (_Keys.ContainsKey(key))
                {
                    string output = null;
                    while (_Keys.ContainsKey(key))
                    {
                        _Keys.TryRemove(key, out output);
                    }
                    return true;
                }
                else
                {
                    foreach (var child in _Childs.Keys)
                    {
                        if (_Childs[child].RemoveKey(key))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            private bool RemoveTag(out List<string> result, int idx, params string[] tags)
            {
                result = null;
                TagTreeItem item;
                if (_Childs.TryGetValue(tags[idx], out item))
                {
                    if (idx == tags.Length - 1)
                    {
                        result = item.Keys.Keys.ToList();
                        while (_Childs.ContainsKey(tags[idx]))
                        {
                            _Childs.TryRemove(tags[idx], out item);
                        }
                        return true;
                    }
                    else
                    {
                        return item.RemoveTag(out result, idx + 1, tags);
                    }
                }
                return false;
            }

            public List<string> RemoveTag(params string[] tags)
            {
                List<string> result = null;
                if (tags != null && tags.Length > 0)
                {
                    RemoveTag(out result, 0, tags);
                }
                return result;
            }

            private bool GetByTag(out List<string> result, int idx, params string[] tags)
            {
                result = null;
                TagTreeItem item;
                if (_Childs.TryGetValue(tags[idx], out item))
                {
                    if (idx == tags.Length - 1)
                    {
                        result = item.Keys.Keys.ToList();
                        return true;
                    }
                    else
                    {
                        return item.GetByTag(out result, idx + 1, tags);
                    }
                }
                return false;
            }

            public List<string> GetByTag(params string[] tags)
            {
                List<string> result = null;
                if (tags != null && tags.Length > 0)
                {
                    GetByTag(out result, 0, tags);
                }
                else
                {
                    result = _Keys.Keys.ToList();
                }
                return result;
            }
        }
        [Serializable]
        private class CacheItem
        {
            public TimeSpan? SlidingExpiration;
            public DateTime AbsoluteExpiration;
            public DateTime Added;
            public string Key;
            public object Value;
        }
        [Serializable]
        private class CacheRepository
        {
            ConcurrentDictionary<string, CacheItem> _Items;

            public ConcurrentDictionary<string, CacheItem> Items
            {
                get { return _Items; }
            }
            TagTreeItem _RootTag;

            public TagTreeItem RootTag
            {
                get { return _RootTag; }
            }

            public CacheRepository()
            {
                _Items = new ConcurrentDictionary<string, CacheItem>();
                _RootTag = new TagTreeItem("ROOT");
            }

            public void Clear()
            {
                _Items.Clear();
                _RootTag.Clear();
            }
        }
        private static readonly object locker = new object();
        private static bool Cleaning = false;
        // CLR guarantees thread-safety during initialization
        private static CacheRepository CacheRepository0 = new CacheRepository();
        private static CacheRepository CacheRepository1 = new CacheRepository();
        private static CacheRepository CurrentRepository = CacheRepository0;
        private static bool SwapRepository = false;
        public delegate object CacheLoaderDelegate();
        private static int SecondsInCache = 3600;


        private static void InvalidateCache()
        {
            if (!Cleaning)
            {
                lock (locker)
                {
                    if (!Cleaning)
                    {
                        Cleaning = true;
                        if (SwapRepository)
                        {
                            CurrentRepository = CacheRepository0;
                            CacheRepository1.Clear();
                        }
                        else
                        {
                            CurrentRepository = CacheRepository1;
                            CacheRepository0.Clear();
                        }
                        SwapRepository = !SwapRepository;
                        Cleaning = false;
                    }
                }
            }
        }


        private static TimeSpan slidingExpiration;
        public static TimeSpan SlidingExpiration
        {
            get
            {
                if (slidingExpiration == default(TimeSpan))
                {
                    lock (locker)
                    {
                        slidingExpiration = new TimeSpan(10, 0, 0);
                    }
                }
                return slidingExpiration;
            }
            set
            {
                lock (locker)
                {
                    slidingExpiration = value;
                }
            }
        }

        /// <summary>
        /// Obtener el objeto almacenado en el cache
        /// </summary>
        /// <param name="clave">clave de acceso al objeto en el cache</param>
        /// <returns></returns>
        public static object Get(string key)
        {
            CacheItem current = null;
            if (CurrentRepository.Items.TryGetValue(key, out current) && current != null)
            {
                if (current.AbsoluteExpiration < DateTime.Now)
                {
                    Delete(key);
                    return null;
                }
                if (current.SlidingExpiration != null)
                {
                    current.AbsoluteExpiration = DateTime.Now + current.SlidingExpiration.Value;
                }
                return current.Value;
            }
            return null;
        }

        public static List<object> GetByTag(params string[] tags)
        {
            List<object> result = new List<object>();
            List<string> keys = CurrentRepository.RootTag.GetByTag(tags);
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    CacheItem current = null;
                    if (CurrentRepository.Items.TryGetValue(key, out current) && current != null)
                    {
                        if (current.AbsoluteExpiration < DateTime.Now)
                        {
                            Delete(key);
                            continue;
                        }
                        if (current.SlidingExpiration != null)
                        {
                            current.AbsoluteExpiration = DateTime.Now + current.SlidingExpiration.Value;
                        }
                        result.Add(current.Value);
                    }
                }
            }
            return result;
        }

        public static void Reset()
        {
            InvalidateCache();
        }

        public static T Get<T>(string key) where T : class
        {
            return Get(key) as T;
        }

        public static List<T> GetByTag<T>(params string[] tags) where T : class
        {
            List<object> items = GetByTag(tags);
            List<T> result = new List<T>();
            if (items != null && items.Count > 0)
            {
                foreach (var item in items)
                {
                    T typeditem = item as T;
                    if (typeditem != null)
                    {
                        result.Add(typeditem);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Guardar el objeto en el cache segun la duracion default en cache configurada
        /// </summary>
        /// <param name="clave">clave de acceso al objeto en el cache</param>
        /// <param name="item">objeto aser guardado</param>
        /// <returns></returns>
        public static void Save(string key, object item, params string[] tags)
        {
            if (Cleaning)
            {
                return;
            }
            if (item != null)
            {
                CurrentRepository.Items.AddOrUpdate(key,
                     (nkey) =>
                     {
                         CacheItem newitem = new CacheItem();
                         newitem.SlidingExpiration = null;
                         newitem.Added = DateTime.Now;
                         newitem.AbsoluteExpiration = DateTime.Now.AddSeconds(SecondsInCache);
                         newitem.Key = nkey;
                         newitem.Value = item;
                         CurrentRepository.RootTag.Add(key, tags);
                         return newitem;
                     },
                     (ukey, oldobj) =>
                     {
                         oldobj.Value = item;
                         oldobj.AbsoluteExpiration = DateTime.Now.AddSeconds(SecondsInCache);
                         return oldobj;
                     });
            }
        }

        /// <summary>
        /// Guardar el objeto en el cache segun la duracion default en cache configurada solo si ya no se encuentra en el cache
        /// </summary>
        /// <param name="clave">clave de acceso al objeto en el cache</param>
        /// <param name="loader">delegadoa funcion que devuelve el valor guardar en caso de que no se encuentre en el cache</param>
        /// <returns></returns>
        public static object Save(string key, CacheLoaderDelegate loader, params string[] tags)
        {
            if (Cleaning)
            {
                return loader();
            }
            CacheItem Obj = CurrentRepository.Items.GetOrAdd(key, (nkey) =>
            {
                CacheItem newitem = new CacheItem();
                newitem.SlidingExpiration = SlidingExpiration;
                newitem.Added = DateTime.Now;
                newitem.AbsoluteExpiration = DateTime.Now + SlidingExpiration;
                newitem.Key = nkey;
                newitem.Value = loader();
                CurrentRepository.RootTag.Add(key, tags);
                return newitem.Value != null ? newitem : null;
            });
            return (Obj != null) ? Obj.Value : null;
        }


        /// <summary>
        /// Guardar el objeto en el cache durante los segundos indicados solo si ya no se encuentra en el cache
        /// </summary>
        /// <param name="clave">clave de acceso al objeto en el cache</param>
        /// <param name="loader">delegadoa funcion que devuelve el valor guardar en caso de que no se encuentre en el cache</param>
        /// <param name="duracion">segundos en cache</param>
        /// <returns></returns>
        public static object Save(string key, CacheLoaderDelegate loader, int SecondsInCache, params string[] tags)
        {
            if (Cleaning)
            {
                return loader();
            }
            CacheItem Obj = CurrentRepository.Items.GetOrAdd(key, (nkey) =>
            {
                CacheItem newitem = new CacheItem();
                newitem.SlidingExpiration = null;
                newitem.Added = DateTime.Now;
                newitem.AbsoluteExpiration = DateTime.Now.AddSeconds(SecondsInCache);
                newitem.Key = nkey;
                newitem.Value = loader();
                CurrentRepository.RootTag.Add(key, tags);
                return newitem.Value != null ? newitem : null;
            });
            return (Obj != null) ? Obj.Value : null;
        }

        /// <summary>
        /// Guardar el objeto en el cache durante los segundos indicados, y cada vez que se lo pide
        /// el tiempo se reiniciara. solo si ya no se encuentra en el cache
        /// </summary>
        /// <param name="clave">clave de acceso al objeto en el cache</param>
        /// <param name="loader">delegadoa funcion que devuelve el valor guardar en caso de que no se encuentre en el cache</param>
        /// <param name="slidingExpiration">tiempo que debe pasar sin que nadie acceda para que se borre de la cache </param>
        /// <returns></returns>
        public static object Save(string key, CacheLoaderDelegate loader, TimeSpan slidingExpiration, params string[] tags)
        {
            if (Cleaning)
            {
                return loader();
            }
            CacheItem Obj = CurrentRepository.Items.GetOrAdd(key, (nkey) =>
            {
                CacheItem newitem = new CacheItem();
                newitem.SlidingExpiration = slidingExpiration;
                newitem.Added = DateTime.Now;
                newitem.AbsoluteExpiration = DateTime.Now + slidingExpiration;
                newitem.Key = nkey;
                newitem.Value = loader();
                CurrentRepository.RootTag.Add(key, tags);
                return newitem.Value != null ? newitem : null;
            });
            return (Obj != null) ? Obj.Value : null;
        }



        /// <summary>
        /// Borra el objeto del cache
        /// </summary>
        /// <param name="clave">clave  de acceso al objeto en el cache</param>
        public static void Delete(string key)
        {
            CacheItem item = null;
            while (CurrentRepository.Items.ContainsKey(key))
            {
                CurrentRepository.Items.TryRemove(key, out item);
            }
            CurrentRepository.RootTag.RemoveKey(key);
        }

        public static void DeleteByTag(params string[] tags)
        {
            List<string> keys = CurrentRepository.RootTag.RemoveTag(tags);
            if (keys != null && keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    CacheItem item = null;
                    while (CurrentRepository.Items.ContainsKey(key))
                    {
                        CurrentRepository.Items.TryRemove(key, out item);
                    }
                }
            }
        }
    }
}
