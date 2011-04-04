using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.Utilities;
using Palaso.Xml;

namespace SIL.LiftBridge.Services
{
	internal class LiftFileServices
	{
		private static readonly Encoding _enc = Encoding.UTF8;

		/// <summary>
		/// Make sure the newly exported LIFT file:
		///		1) conforms to the Palas0 canonical XML writer settings, and
		///		2) retains the order of entries in the original file.
		///
		/// Both of these adjustments are needed to make life easier on Mercurial.
		/// </summary>
		/// <param name="originalPathname">The original LIFT file found in Mercurial.</param>
		/// <param name="tempPathname">The newly exported LIFT file from FLEx.</param>
		internal static void PrettyPrintFile(string originalPathname, string tempPathname)
		{
			var bakPathname = originalPathname + ".bak";
			File.Copy(originalPathname, bakPathname, true); // Be safe in this.

			// Diff the original file (now bak) and the newly exported file (temp).
			//var differ = Xml2WayDiffer.CreateFromFiles(bakPathname, tempPathname, new NullMergeEventListener(), "header", "entry", "id");
			//Dictionary<string, byte[]> childIndex;
			//var parentIndex = differ.ReportDifferencesToListener(out childIndex);
			var parentIndex = new Dictionary<string, byte[]>();
			using (var parentPrepper = new DifferDictionaryPrepper(parentIndex, bakPathname, "header", "entry", "id"))
			{
				parentPrepper.Run();
			}
			var childIndex = new Dictionary<string, byte[]>();
			using (var childPrepper = new DifferDictionaryPrepper(childIndex, tempPathname, "header", "entry", "id"))
			{
				childPrepper.Run();
			}

			// Collect up the new entries.
			var parentKeys = parentIndex.Keys.ToList();
			var newbies = new HashSet<string>(from childKey in childIndex.Keys
											  where !parentKeys.Contains(childKey)
												select childKey,
											  StringComparer.OrdinalIgnoreCase);
			// Collect the ones that were deleted.
			var childKeys = childIndex.Keys.ToList();
			var goners = new HashSet<string>(from parentKey in parentIndex.Keys
											 where !childKeys.Contains(parentKey)
												select parentKey,
											 StringComparer.OrdinalIgnoreCase);

			// Write the entries in the order of records in the new '.bak' file. New entries can get appended to the end.
			using (var writer = XmlWriter.Create(originalPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				// 1. Start root element.
				writer.WriteStartElement("lift");

				// 2. Write root element attrs from *temp* file, since it is the latest.
				using (var tempReader = XmlReader.Create(tempPathname, new XmlReaderSettings {IgnoreWhitespace = true}))
				{
					tempReader.MoveToContent();
					for (var i = 0; i < tempReader.AttributeCount; ++i)
					{
						tempReader.MoveToAttribute(i);
						writer.WriteAttributeString(tempReader.LocalName, tempReader.Value);
					}
				}

				// 3. Write all child elements, including optional 'header'.
				using (var reader = XmlReader.Create(bakPathname, new XmlReaderSettings { IgnoreWhitespace = true }))
				{
					reader.MoveToContent();
					// Write all root element child elements.
					// TODO?: While we have the files opened, then take care of the Flex export issue, where senses end up with guid_guid (or gloss_guid) ids.
					// Delete goners.
					// Copy extant objects to new file using verison (same or modified, no matter) from childIndex.
					// Add newbies at end.
					var handledHeader = false;
					var keepReading = reader.Read();
					while (keepReading)
					{
						var currentId = handledHeader ? reader.GetAttribute("id") : "header";
						// Optional header element.
						if (!handledHeader)
						{
							byte[] headerToWrite;
							childIndex.TryGetValue(currentId, out headerToWrite);
							if (reader.LocalName == currentId)
								reader.ReadToNextSibling("entry");

							// If 'header' is in childIndex, then add it now.
							if (headerToWrite != null)
								WriteNode(writer, headerToWrite);
							RemoveItem(currentId, childIndex, newbies, goners);
							handledHeader = true;
						}
						else
						{
							if (reader.EOF)
								break; // Empty file. Just add any newbies.

							// 'entry' node is current node in reader.
							// Fetch id string from 'entry' node and see if it is in the deleted dictionary.
							if (goners.Contains(currentId))
							{
								// Skip this record, since it has been deleted.
								reader.ReadToNextSibling("entry");
								RemoveItem(currentId, childIndex, newbies, goners);
								// continue;
							}
							else
							{
								// If they were not removed, then they are extant, so use version from childIndex,
								// which may be the same or different, but that is not relevant.
								// Write out childIndex version.
								// Skip over reader (parentIndex) version
								reader.ReadOuterXml();
								// and use childIndex version
								WriteNode(writer, childIndex[currentId]);
							}
						}
						keepReading = reader.IsStartElement();
					}
				}

				// 4. Add all new records.
				foreach (var newbyKey in newbies)
					WriteNode(writer, childIndex[newbyKey]);

				// Close root element.
				writer.WriteEndElement();
			}
			File.Delete(tempPathname);
			File.Delete(bakPathname);
		}

		private static void WriteNode(XmlWriter writer, byte[] dataToWrite)
		{
			var doc = new XmlDocument();
			doc.LoadXml(_enc.GetString(dataToWrite));
			writer.WriteNode(doc.CreateNavigator(), true);
		}

		private static void RemoveItem(string key, IDictionary<string, byte[]> childIndex, ICollection<string> newbies, ICollection<string> goners)
		{
			childIndex.Remove(key);
			newbies.Remove(key);
			goners.Remove(key);
		}
	}
}
