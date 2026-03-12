declare module "foliate-js/pdf.js" {
  export function makePDF(file: Blob): Promise<unknown>;
}

declare module "foliate-js/vendor/pdfjs/pdf.mjs";
declare module "../../../node_modules/foliate-js/vendor/pdfjs/pdf.mjs";
