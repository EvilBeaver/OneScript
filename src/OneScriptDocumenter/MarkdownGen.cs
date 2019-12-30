using MarkdownDeep;

namespace OneScriptDocumenter
{
    class MarkdownGen : MarkdownDeep.Markdown
    {
        public override void OnPrepareLink(HtmlTag tag)
        {
            tag.attributes["href"] = tag.attributes["href"] + ".htm";
            base.OnPrepareLink(tag);
        }
    }
}
