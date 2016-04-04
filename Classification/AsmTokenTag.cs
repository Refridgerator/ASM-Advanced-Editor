using System.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AsmAE
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("asm")]
    [TagType(typeof(AsmTokenTag))]
    internal sealed class AsmTokenTagProvider : ITaggerProvider
    {

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new AsmTokenTagger(buffer) as ITagger<T>;
        }
    }

    public class AsmTokenTag : ITag 
    {
        public AsmTokens type { get; private set; }

        public AsmTokenTag(AsmTokens type)
        {
            this.type = type;
        }
    }

    internal sealed class AsmTokenTagger : ITagger<AsmTokenTag>
    {
        ITextBuffer _buffer;
        IDictionary<string, AsmTokens> _asmTypes;
        Regex rx_number = new Regex(@"^(-?\d+(\.\d+)?|[01]+[bB]|[01234567]+[oO]|[0-9a-fA-F]+[hH])$", RegexOptions.Compiled);

        internal AsmTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _asmTypes = new Dictionary<string, AsmTokens>();

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetName().Name + "." + "keywords.ini";
            string line;
            AsmTokens classification = AsmTokens.TEXT;
            using (Stream file = assembly.GetManifestResourceStream(resourceName))
            using (TextReader textReader = new StreamReader(file))
                while ((line = textReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.IndexOf("[") > -1) // new section
                    {
                        classification = (AsmTokens)Enum.Parse(typeof(AsmTokens), 
                            line.Trim("[]".ToCharArray()));
                        continue;

                    }
                    if (line == "") continue;
                    
                    line = line.ToLower();                    
                    if (!_asmTypes.ContainsKey(line))
                     {
                         _asmTypes.Add(line, classification);
                     }
                     else
                     {
                         Trace.WriteLine(string.Format("Warning: token {0} for classification {1} already added.", line, classification));
                     }
                }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<AsmTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            List<TagSpan<AsmTokenTag>> tags = new List<TagSpan<AsmTokenTag>>();
            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine textSnapshotLine = curSpan.Start.GetContainingLine();
                string text_line = textSnapshotLine.GetText().ToLower();
                int cur_pos = textSnapshotLine.Start.Position;
                // very slow
                int comment_pos = text_line.IndexOf(';');
                if(comment_pos>-1) text_line = text_line.Substring(0, comment_pos);
                string[] tokens = text_line.Split(" \t,[]+-:*".ToCharArray());

                foreach (string asmToken in tokens)
                {
                    if (asmToken == "")
                    {
                        cur_pos += asmToken.Length + 1;
                        continue;
                    }
                    if (_asmTypes.ContainsKey(asmToken))
                    {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(cur_pos, asmToken.Length));
                        if (tokenSpan.IntersectsWith(curSpan))
                            tags.Add(new TagSpan<AsmTokenTag>(tokenSpan, new AsmTokenTag(_asmTypes[asmToken])));
                    }
                    else
                    {
                        if (rx_number.IsMatch(asmToken))
                        {
                            var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(cur_pos, asmToken.Length));
                            if (tokenSpan.IntersectsWith(curSpan))
                                tags.Add(new TagSpan<AsmTokenTag>(tokenSpan, new AsmTokenTag(AsmTokens.NUMBER)));
                        }
                    }

                    //add an extra char location because of the space
                    cur_pos += asmToken.Length + 1;
                }
                if (comment_pos > -1)
                {
                    var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(cur_pos-1, textSnapshotLine.Length - comment_pos));
                    if (tokenSpan.IntersectsWith(curSpan))
                        tags.Add(new TagSpan<AsmTokenTag>(tokenSpan, new AsmTokenTag(AsmTokens.COMMENT_LINE)));
                }
            }
            foreach (var tag in tags)
                yield return tag;
        }
    }
}
