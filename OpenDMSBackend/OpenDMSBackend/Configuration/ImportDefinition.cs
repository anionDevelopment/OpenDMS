namespace OpenDMSBackend.Core.Configuration
{
    public class ImportDefinition
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string SourceLocationURL { get; set; }
        public string TargetFolderId { get; set; }
        /// <summary>
        /// The body of an optional TypeScript-script which adapts every document which is imported by this import-definition, or <see langword="null"/> when the imported documents are taken as they are.
        /// The script-body is inserted into the method <c>Runner.adapt(document: Document)</c> of the script-template <c>Resources/Typescript/AdaptDocument.ts</c>, so it can change the writable properties of <c>document</c>
        /// (<c>Title</c>, <c>Filename</c>, <c>Tags</c>, <c>GroupOfBusinessOwner</c>, <c>DeleteIsNotAllowedBefore</c> and <c>MustBeHardDeletedAfter</c>) and read the immutable ones (for example <c>OriginalFilename</c>, <c>ImportDate</c>, <c>ReadableId</c>, <c>MIMEType</c> and <c>OCRContent</c>).
        /// A tag is resolved by its name with <c>tools.getTagByName(name)</c>, which only knows the global tags because an import is an automatic system-operation, and which throws a <c>RangeError</c> for an unknown name.
        /// </summary>
        /// <example>
        /// document.Title = "Document_" + document.ReadableId + "_" + document.ImportDate.toISOString();
        /// document.GroupOfBusinessOwner = "OU73";
        /// document.Tags = document.Tags.concat([tools.getTagByName("Imported")]);
        /// </example>
        public string? AdaptDocumentScriptBody { get; set; }
    }
}
