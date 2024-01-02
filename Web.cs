﻿using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

namespace CoreUtils
{
    public static class Web
    {   
        public static string MergeQueryString (string url, params (string, string)[] nameValues) {
            NameValueCollection qstring = new NameValueCollection();
            foreach (var t in nameValues) {
                qstring[t.Item1] = t.Item2;
            }            
            return MergeQueryString(url, qstring);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">http://www.mywebsite.com/index.aspx?id=1&categoryid=2</param>
        /// <param name="qstring">facilityid=3</param>
        /// <returns>http://www.mywebsite.com/index.aspx?id=1&categoryid=2&facilityid=3</returns>
        public static string MergeQueryString(string url, string qstring)
        {
            var nv = System.Web.HttpUtility.ParseQueryString(qstring);
            return MergeQueryString(url, nv, true);
        }

        public static string MergeQueryString(string url, NameValueCollection qstring, bool keepall = true)
        {
            string query = "";
            string[] arr = url.Split(new Char[] { '?' });
            NameValueCollection nv = new NameValueCollection();
            url = arr[0];

            if (keepall)
            {
                //keep the existing query string values
                if (arr.Length > 1)
                {
                    query = arr[1];
                    nv = System.Web.HttpUtility.ParseQueryString(query);
                }
            }

            string newq = "";
            //loop through new query
            for (int i = 0; i < qstring.Count; i++)
            {
                if (!string.IsNullOrEmpty(nv[qstring.GetKey(i)])) //update
                {
                    nv[qstring.GetKey(i)] = qstring[i];
                }
                else
                {
                    nv.Add(qstring.GetKey(i), qstring[i]); //add
                }
            }

            //construct new query string
            for (int i = 0; i < nv.Count; i++)
            {
                if (!string.IsNullOrEmpty(nv[i]))
                {
                    newq += nv.GetKey(i) + "=" + nv[i] + "&";
                }
            }

            if (newq.Length > 0)
            {
                //remove last &
                newq = newq.Substring(0, newq.Length - 1);
                return url + "?" + newq;
            }

            return url;
        }
        
        public static NameValueCollection GetNV(NameValueCollection qstring) {
            NameValueCollection nv = new NameValueCollection();            
            foreach (string key in qstring.Keys) {
                nv[key] = System.Web.HttpUtility.UrlEncode(qstring[key]);
            }
            return nv;
        }
    }
}
