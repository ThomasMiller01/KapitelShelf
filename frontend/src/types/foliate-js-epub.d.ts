declare module "foliate-js/epub.js"
{
    export interface FoliateSection
    {
        id: string;
        load: () => Promise<string>;
        unload?: () => void;
        createDocument?: () => Promise<Document>;
        size: number;
        cfi?: string;
        linear?: string;
        pageSpread?: string;
        resolveHref?: (href: string) => string;
        mediaOverlay?: unknown;
    }

    export interface FoliateBook
    {
        sections: FoliateSection[];
        metadata?: unknown;
        toc?: unknown[];
        pageList?: unknown[];
        landmarks?: unknown[];
        resolveCFI?: (cfi: string) => unknown;
        resolveHref?: (
            href: string
        ) => {
            index: number;
            anchor?: (doc: Document) => unknown;
        } | null;
        destroy?: () => void;
    }

    export interface EPUBConstructorOptions
    {
        loadText: (path: string) => Promise<string>;
        loadBlob: (path: string) => Promise<Blob>;
        getSize: (path: string) => number;
        sha1?: (value: string) => Promise<Uint8Array>;
    }

    export class EPUB
    {
        constructor(options: EPUBConstructorOptions);

        metadata?: unknown;
        toc?: unknown[];
        pageList?: unknown[];
        landmarks?: unknown[];
        sections: FoliateSection[];

        init(): Promise<FoliateBook>;
        resolveCFI(cfi: string): unknown;
        resolveHref(
            href: string
        ): {
            index: number;
            anchor?: (doc: Document) => unknown;
        } | null;
        destroy(): void;
    }
}