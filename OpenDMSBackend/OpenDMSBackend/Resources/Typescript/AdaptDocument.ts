class Tag {
    Id: string;
    Name: string;
    constructor(id: string,name: string) {
        {
            this.Id = id;   
            this.Name = name;
        }
}
class Document {
    readonly Id: string;
    Title: string;
    Filename: string;
    readonly OriginalFilename: string;
    readonly ImportDate: Date;
    readonly Tags: Set<Tag>;
    readonly ReadableId: bigint;
    readonly MIMEType: string;
    readonly OCRContent: string;
    DeleteIsNotAllowedBefore: Date;
    MustBeHardDeletedAfter: Date;
    GroupOfBusinessOwner: string;
    readonly AddedByUserId: string;

    constructor(id: string, title: string, filename: string, originalFilename: string, importDate: Date, tags: Set<Tag>, readableId: bigint, mimeType: string, ocrContent: string, deleteIsNotAllowedBefore: Date, mustBeHardDeletedAfter: Date, groupOfBusinessOwner: string, addedByUserId:string) {
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.Tags = tags;
            this.MIMEType = mimeType;
            this.OCRContent = ocrContent;
            this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
            this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
            this.GroupOfBusinessOwner = groupOfBusinessOwner;
            this.AddedByUserId = addedByUserId;
        }
    }
}

interface ITools {
    getTagByName(name: string): Tag;
}

class Tools implements ITools {
    getTagByName(name: string): Tag {
        //<tag-definitions>
        throw new RangeError("Unknown tag-name: " + name);
    }
}

class Runner {
    private readonly tools: ITools;
    constructor(tools: ITools) {
        this.tools = tools;
    }

    adapt(document: Document): void {
        //<custom-script>
    };
}
