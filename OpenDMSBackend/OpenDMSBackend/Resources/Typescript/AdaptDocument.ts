class Tag {
    Id: string;
    Name: string;
    constructor(id: string, name: string) {
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
    Tags: Tag[];
    readonly ReadableId: number;
    readonly MIMEType: string;
    readonly OCRContent: string;
    DeleteIsNotAllowedBefore: Date | null;
    MustBeHardDeletedAfter: Date | null;
    GroupOfBusinessOwner: string;
    readonly AddedByUserId: string;

    constructor(id: string, title: string, filename: string, originalFilename: string, importDate: Date, tags: Tag[], readableId: number, mimeType: string, ocrContent: string, deleteIsNotAllowedBefore: Date | null, mustBeHardDeletedAfter: Date | null, groupOfBusinessOwner: string, addedByUserId: string) {
        this.Id = id;
        this.Title = title;
        this.Filename = filename;
        this.OriginalFilename = originalFilename;
        this.ImportDate = importDate;
        this.Tags = tags;
        this.ReadableId = readableId;
        this.MIMEType = mimeType;
        this.OCRContent = ocrContent;
        this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
        this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
        this.GroupOfBusinessOwner = groupOfBusinessOwner;
        this.AddedByUserId = addedByUserId;
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
        const tools: ITools = this.tools;
        //<custom-script>
    }
}
