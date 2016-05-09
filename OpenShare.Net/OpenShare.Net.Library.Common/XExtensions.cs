using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OpenShare.Net.Library.Common
{
    public static class XExtensions
    {
        /// <summary>
        /// Converts the XmlNode to an XElement via an XmlWriter.
        /// Source pulled and modified from:
        /// http://blogs.msdn.com/b/ericwhite/archive/2008/12/22/convert-xelement-to-xmlnode-and-convert-xmlnode-to-xelement.aspx
        /// </summary>
        /// <param name="xmlNode">The XmlNode to convert to an XElement.</param>
        /// <returns>Returns an XElement form of the current XmlNode.</returns>
        public static XElement GetXElement(this XmlNode xmlNode)
        {
            var xDocument = new XDocument();
            using (var xmlWriter = xDocument.CreateWriter())
                xmlNode.WriteTo(xmlWriter);
            return xDocument.Root;
        }

        /// <summary>
        /// Converts an XNode into an XElement via XElement.Parse.
        /// Source pulled and modified from:
        /// http://codesnippets.fesslersoft.de/how-to-convert-xnode-to-xelement-in-c-and-vb-net/
        /// </summary>
        /// <param name="xNode">The XNode to convert into an XElement.</param>
        /// <returns>Returns an XElement form of the current XNode.</returns>
        public static XElement GetXElement(this XNode xNode)
        {
            return XElement.Parse(xNode.ToString());
        }

        /// <summary>
        /// Converts the XElement to an XmlNode via an XmlReader.
        /// Source pulled and modified from:
        /// http://blogs.msdn.com/b/ericwhite/archive/2008/12/22/convert-xelement-to-xmlnode-and-convert-xmlnode-to-xelement.aspx
        /// </summary>
        /// <param name="xElement">The XElement to convert to an XmlNode.</param>
        /// <returns>Returns an XmlNode form of the current XElement.</returns>
        public static XmlNode GetXmlNode(this XElement xElement)
        {
            using (var xmlReader = xElement.CreateReader())
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlReader);
                return xmlDocument;
            }
        }

        /// <summary>
        /// Converts the XmlElement to an XDocument, assuming the XmlElement
        /// represents a full XML document.
        /// </summary>
        /// <param name="xmlElement">The XElement to convert to an XDocument.</param>
        /// <returns>Returns an XDocument form of the current XmlElement.</returns>
        public static XDocument GetXDocument(this XmlElement xmlElement)
        {
            return XDocument.Load(xmlElement.CreateNavigator().ReadSubtree());
        }

        /// <summary>
        /// Converts an XmlElement to the Type T assuming that
        /// the XmlElement paramter, xmlElement, represents a
        /// full XML Document.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="xmlElement">The XmlElement to convert to type T.</param>
        /// <returns>The an XmlElement as type T.</returns>
        public static T GetType<T>(this XmlElement xmlElement)
        {
            var serializer = new XmlSerializer(typeof(T));
            var xDocument = xmlElement.GetXDocument();
            if (xDocument.Root == null)
                throw new Exception("XmlElement parameter, xmlElement, did not represent a full XML Document.");
            var xmlReader = xDocument.Root.CreateReader();
            return (T)serializer.Deserialize(xmlReader);
        }

        /// <summary>
        /// Converts an XmlElement to the Type T assuming that
        /// the XmlElement paramter, xmlElement, represents a
        /// full XML Document.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="xmlElement">The XmlElement to convert to type T.</param>
        /// <param name="elementName">
        /// The XmlRootAttribute has an ElementName property that can be
        /// declared here if it has not yet been declared in the XmlRootAttribute
        /// on type T.
        /// </param>
        /// <returns>The an XmlElement as type T.</returns>
        public static T GetType<T>(this XmlElement xmlElement, string elementName)
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(elementName));
            var xDocument = xmlElement.GetXDocument();
            if (xDocument.Root == null)
                throw new Exception("XmlElement parameter, xmlElement, did not represent a full XML Document.");
            var xmlReader = xDocument.Root.CreateReader();
            return (T)serializer.Deserialize(xmlReader);
        }
    }
}
