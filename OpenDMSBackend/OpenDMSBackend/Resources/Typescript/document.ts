class Tag {
    Id: string;
    Name: string;
}
class Document {
    readonly Id: string;
    Title: string;
    Filename: string;
    readonly OriginalFilename: string;
    readonly ImportDate: Date;
    Tags: Set<Tag>;
    readonly ReadableId: bigint;
    readonly MIMEType: string;
    readonly OCRContent: string;
    DeleteIsNotAllowedBefore: string;
    MustBeHardDeletedAfter: string;
    GroupOfBusinessOwner: string;

    constructor(title: string, importDate: Date) {
        {
            this.Title = title;
            this.ImportDate = importDate;
        }
    }
}
interface Tools {
    getTagByName(name: string): Tag;
}
class Runner {
    private tools: Tools;
    constructor(tools: Tools) {
        this.tools = tools;
    }

    adapt(document: Document, ): Document {
        {
            //<your-custom-script>
            return document;
        }
    };
}