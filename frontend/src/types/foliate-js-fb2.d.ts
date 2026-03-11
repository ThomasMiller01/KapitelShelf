declare module "foliate-js/fb2.js"
{
    export interface Fb2TocItem
    {
        label?: string;
        href?: string;
        subitems?: Fb2TocItem[] | null;
    }

    export interface Fb2Section
    {
        id: number;
        load: () => string; // synchronous — returns blob URL directly
        createDocument?: () => Document;
        size: number;
        linear?: "no";
    }

    export interface Fb2Metadata
    {
        title?: string;
        identifier?: string;
        language?: string;
        author?: { name?: string; sortAs?: string | null }[];
        translator?: { name?: string; sortAs?: string | null }[];
        contributor?: { name?: string; sortAs?: string | null; role?: string }[];
        publisher?: string;
        published?: string;
        modified?: string;
        description?: string | null;
        subject?: string[];
    }

    export interface Fb2Book
    {
        metadata?: Fb2Metadata;
        toc?: Fb2TocItem[];
        sections: Fb2Section[];
        getCover: () => Promise<Blob | null>;
        resolveHref: (href: string) => {
            index: number;
            anchor?: (doc: Document) => Element | null;
        };
        splitTOCHref: (href: string) => number[];
        getTOCFragment: (doc: Document, id: number) => Element | null;
        destroy: () => void;
    }

    export function makeFB2(blob: Blob): Promise<Fb2Book>;
}
