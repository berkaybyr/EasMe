﻿using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EasMe.Extensions
{
    public static class XmlExtensions
    {
        /// <summary>
        /// Deserializes given XElement to T type object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xElement"></param>
        /// <returns></returns>
        /// <exception cref="EasException"></exception>
        public static T? XmlDeserialize<T>(this XElement xElement)
        {
            StringReader reader = new(xElement.ToString().Replace("True", "true").Replace("False", "false"));
            XmlSerializer xmlSerializer = new(typeof(T));
            var item = (T?)xmlSerializer.Deserialize(reader);
            return item;
        }
        /// <summary>
        /// Deserializes given IEnumerable of XElement to T type object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xElements"></param>
        /// <returns></returns>
        /// <exception cref="EasException"></exception>
        public static List<T> XmlDeserialize<T>(this IEnumerable<XElement> xElements)
        {
            var list = new List<T>();
            Parallel.ForEach(xElements, el =>
            {
                var item = el.XmlDeserialize<T>();
                if (item != null)
                {
                    lock (list)
                    {
                        list.Add(item);
                    }
                }

            });
            return list;
        }
        public static string ToXML<T>(this T t)
        {
            using var stringwriter = new StringWriter();
            var serializer = new XmlSerializer(t.GetType());
            serializer.Serialize(stringwriter, t);
            return stringwriter.ToString();
            
        }
        public static string ToCleanXml<T>(this T value)
        {
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(value.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using var stream = new StringWriter();
            using var writer = XmlWriter.Create(stream, settings);
            serializer.Serialize(writer, value, emptyNamespaces);
            return stream.ToString();
        }
        public static XElement ToXElement<T>(this T obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter streamWriter = new StreamWriter(memoryStream))
                {
                    var xmlSerializer = new XmlSerializer(typeof(T));
                    xmlSerializer.Serialize(streamWriter, obj);
                    return XElement.Parse(Encoding.ASCII.GetString(memoryStream.ToArray()));
                }
            }
        }

        public static XElement? ToXML<T>(this T t, string elementName, bool asAtrribute = false)
        {

            var docElement = new XElement(elementName);
            var type = t?.GetType();
            var properties = type?.GetProperties();
            if (properties == null) return default;
            foreach (var property in properties)
            {
                var value = property.GetValue(t);
                if (value != null)
                {
                    if (asAtrribute)
                    {
                        var element = new XAttribute(property.Name, value);
                        docElement.Add(element);
                    }
                    else
                    {
                        var element = new XElement(property.Name, value);
                        docElement.Add(element);
                    }


                }
            }
            return docElement;
        }
    }
}