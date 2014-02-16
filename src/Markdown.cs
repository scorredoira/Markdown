using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Sfx.Markdown
{	
	/// <summary>
	/// Implementa un subset de Markdown.
	/// </summary>
	public static class MarkdownWritter
	{
		const int DEFAULT = 0;
		const int INSIDE_LIST = 1;
		const int INSIDE_PRE = 4;
		const string LI_TAG = "  * ";
		const string H1_TAG = "=====";
		const string H2_TAG = "-----";

		public static string Write(string text)
		{			
			using(var w = new StringWriter())
			{
				Write(text, w);
				return w.ToString();
			}
		}

		public static void Write(string text, TextWriter writer)
		{
			text = text.Replace ("\r", "");
			var lines = text.Split ('\n');

			int status = DEFAULT;

			for(int i = 0, l = lines.Length; i < l; i++)
			{
				var line = lines[i];

				var isPreformattedBlock = IsPreformattedBlock(ref line, i, lines, status);
				if(isPreformattedBlock && status != INSIDE_PRE)
				{
					writer.WriteLine("<pre>");
					status = INSIDE_PRE;
				}
				else if(!isPreformattedBlock && status == INSIDE_PRE)
				{
					writer.WriteLine("</pre>");
					status = DEFAULT;
				}

				switch(status)
				{
					case INSIDE_PRE:
						InsidePre(line, writer);
						break;
							
					case DEFAULT:
						if(line.StartsWith(LI_TAG))
						{
							InsideList(line, i, lines, ref status, writer);
							writer.Write("\n");
						}
						else
						{
							if(lines.Length > i + 1)
							{
								var nextLine = lines[i + 1];

								if(nextLine.StartsWith(H1_TAG))
								{
									InsideBlockTag(line, ref i, "h1", writer);
									writer.Write("\n");
									break;
								}
								else if(nextLine.StartsWith(H2_TAG))
								{
									InsideBlockTag(line, ref i, "h2", writer);
									writer.Write("\n");
									break;
								}
							}

							if(line != string.Empty)
							{
								Default(line, writer);
							}
						}
						break;

					case INSIDE_LIST:
						InsideList(line, i, lines, ref status, writer);
						writer.Write("\n");
						break;
				}
			}			

			switch(status)
			{
				case INSIDE_LIST:
					writer.WriteLine ("</ul>");
					break;

				case INSIDE_PRE:
					writer.WriteLine ("</pre>");
					break;
			}
		}

		static bool IsPreformattedBlock(ref string line, int lineIndex, string[] lines, int status)
		{
			if(status != INSIDE_PRE)
			{
				if(lineIndex > 0 && lines[lineIndex - 1] != string.Empty)
				{
					return false;
				}
			}

			return line.StartsWith("    ") || line.StartsWith("\t");
		}

		static void InsidePre(string line, TextWriter wr)
		{			
			if(line.StartsWith("    "))
			{
				line = line.Substring(4);
			}
			else if (line.StartsWith("\t"))
			{
				line = line.Substring(1);
			}

			line = EncodeHtmlEntities(line);

			foreach(var c in line)
			{
				switch(c)
				{
					case '\t':
						wr.Write("    ");
						break;

					default:
						wr.Write(c);
						break;
				}
			}

			wr.Write("\n");
		}

		static void InsideList(string line, int lineIndex, string[] lines, ref int status, TextWriter wr)
		{
			if (!line.StartsWith (LI_TAG))
			{
				wr.WriteLine ("</ul>");
				status = DEFAULT;
				Default (line, wr);
				return;
			}
				
			if(status != INSIDE_LIST)
			{
				wr.WriteLine("<ul>");
				status = INSIDE_LIST;
			}

			wr.Write ("<li>");
			wr.Write(EncodeLine(line.Substring(LI_TAG.Length)));
			wr.Write ("</li>");
		}
		
		static void InsideBlockTag(string line, ref int lineIndex, string tag, TextWriter wr)
		{
			wr.Write("<");
			wr.Write(tag);
			wr.Write(">");		
			wr.Write(EncodeLine(line));
			wr.Write("</");
			wr.Write(tag);
			wr.Write(">");	

			lineIndex = lineIndex + 1;
		}
		
		static void Default(string line, TextWriter wr)
		{
			wr.Write(EncodeLine(line));

			wr.Write("\n");

			if(line.EndsWith("  "))
			{
				wr.Write("<br />");
			}
		}
		
		static string EncodeLine(string line)
		{
			line = EncodeHtmlEntities (line);
			line = EncodeLinksAndImages(line);
			line = EncodeTag (line, "**", "strong");
			line = EncodeTag (line, "__", "i");

			// html personalizado
			line = EncodeCustomHtml (line);
			return line;
		}
		
		static string EncodeCustomHtml(string value)
		{			
			value = value.Replace ("{{", "<");
			value = value.Replace("}}", ">");
			return value;
		}
		
		public static string EncodeHtmlEntities(string value)
		{			
			value = value.Replace("<", "&lt;");
			value = value.Replace(">", "&gt;");
			return value;
		}
		
		static string EncodeTag(string line, string token, string htmlTag)
		{
			var start = 0;
			do
			{
				start = line.IndexOf(token, start);
				if(start != -1)
				{
					var tokenLen = token.Length;

					var end = line.IndexOf(token, start + tokenLen);
					if(end != -1)
					{
						var sb = new StringBuilder();

						if(start > -1)
						{
							sb.Append(line.Substring(0, start));
						}
					
						sb.Append("<");
						sb.Append(htmlTag);
						sb.Append(">");					
						sb.Append(line.Substring(start + tokenLen, end - start - tokenLen));
						sb.Append("</");
						sb.Append(htmlTag);
						sb.Append(">");	

						if(line.Length > end + tokenLen)
						{
							sb.Append(line.Substring(end + tokenLen));
						}

						line = sb.ToString();
					}
				}
			}
			while(start != -1);
			
			return line;
		}

		static string EncodeLinksAndImages(string text)
		{
			var sb = new StringBuilder();
			var index = 0;

			foreach(Match match in Regex.Matches(text, @"(\!)?\[([^\]]*)\]\(([^\]]*)\)"))
			{
				// incluir el texto entre el último match y este.
				if(match.Index > index)
				{
					sb.Append(text.Substring(index, match.Index - index));
				}

				if(match.Groups.Count > 1)
				{
					var isImage = match.Groups[1].Value == "!";
					var url = match.Groups[3].Value.Trim();
					var altText = match.Groups[2].Value.Trim();

					if(!string.IsNullOrEmpty(url))
					{
						if(isImage)
						{
							RenderImage(url, altText, sb);
						}
						else
						{
							RenderLink(url, altText, sb);
						}
					}
				}

				// avanzar hasta el final del tag
				index = match.Index + match.Length;
			}

			// si queda texto por procesar, añadirlo
			if(text.Length > index)
			{
				sb.Append(text.Substring(index, text.Length - index));
			}

			return sb.ToString();
		}

		static void RenderLink(string url, string altText, StringBuilder sb)
		{
			if(string.IsNullOrEmpty(altText))
			{
				altText = url;
			}
			sb.Append(string.Format("<a href='{0}'>{1}</a>", url, altText));
		}

		static void RenderImage(string url, string altText, StringBuilder sb)
		{
			if(altText != null)
			{
				sb.Append(string.Format("<img src='{0}' alt='{0}' />", url, altText));
			}
			else
			{ 
				sb.Append(string.Format("<img src='{0}' />", url));
			}
		}
	}
}













































