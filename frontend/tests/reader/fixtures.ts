import JSZip from "jszip";

const paragraph = "The reader followed the river through the village. ".repeat(530);
const xhtml = (body: string): string =>
  `<html xmlns="http://www.w3.org/1999/xhtml"><head><title>Fixture</title></head><body>${body}</body></html>`;

export const epubFixture = async (): Promise<File> => {
  const zip = new JSZip();
  zip.file("mimetype", "application/epub+zip");
  zip.file("META-INF/container.xml", `<container xmlns="urn:oasis:names:tc:opendocument:xmlns:container" version="1.0">
    <rootfiles><rootfile full-path="book.opf" media-type="application/oebps-package+xml"/></rootfiles>
  </container>`);
  zip.file("book.opf", `<package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="id">
    <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
      <dc:identifier id="id">urn:kapitelshelf:fixture</dc:identifier><dc:title>EPUB Fixture</dc:title><dc:language>en</dc:language>
    </metadata>
    <manifest>
      <item id="second" href="second.xhtml" media-type="application/xhtml+xml"/>
      <item id="first" href="first.xhtml" media-type="application/xhtml+xml"/>
      <item id="nav" href="nav.xhtml" properties="nav" media-type="application/xhtml+xml"/>
      <item id="image" href="dot.png" media-type="image/png"/>
    </manifest>
    <spine><itemref idref="first"/><itemref idref="second"/></spine>
  </package>`);
  zip.file("nav.xhtml", `<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops"><body>
    <nav epub:type="toc"><ol>
      <li><a href="first.xhtml">First chapter</a><ol><li><a href="first.xhtml#part">Nested entry</a></li></ol></li>
      <li><a href="second.xhtml">Illustration</a></li>
    </ol></nav>
  </body></html>`);
  zip.file("first.xhtml", xhtml(`<p>${paragraph}</p><p id="part">${paragraph}</p>`));
  zip.file("second.xhtml", xhtml('<img src="dot.png" alt="Packaged illustration"/>'));
  zip.file("dot.png", "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jT2sAAAAASUVORK5CYII=", { base64: true });
  return new File([await zip.generateAsync({ type: "blob" })], "legacy.epub", { type: "application/epub+zip" });
};

export const fb2Fixture = (): File => new File([`<?xml version="1.0" encoding="UTF-8"?>
  <FictionBook xmlns="http://www.gribuser.ru/xml/fictionbook/2.0">
    <description><title-info><book-title>FB2 Fixture</book-title><lang>en</lang></title-info></description>
    <body>
      <section><title><p>First chapter</p></title><p>One short paragraph.</p>
        <section><title><p>Nested entry</p></title><p>Another paragraph.</p></section>
      </section>
      <section><title><p>Second chapter</p></title><p>The chapter ends here.</p></section>
    </body>
    <body name="notes"><section><title><p>Notes</p></title><p>One note.</p></section></body>
  </FictionBook>`], "legacy.fb2", { type: "application/x-fictionbook+xml" });

export const txtFixture = (): File => new File([
  `Preface text that ends with punctuation.\r\n\r\n1\r\nOpening\r\n${paragraph}\r\n\r\n${paragraph}\r\n\r\nFINALE\r\n\r\nThe story ends with café and 雪.`,
], "legacy.txt", { type: "text/plain" });

export const pdfFixture = (textPages = true): File => {
  // Every object contains ASCII so string offsets are also PDF byte offsets.
  const stream = (content: string): string => `<< /Length ${content.length} >>\nstream\n${content}\nendstream`;
  const page = (ref: number): string => `<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents ${ref} 0 R >>`;
  const objects = [
    "<< /Type /Catalog /Pages 2 0 R /Outlines 10 0 R >>",
    "<< /Type /Pages /Kids [4 0 R 6 0 R 8 0 R] /Count 3 >>",
    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
    page(5), stream(textPages ? "BT /F1 12 Tf 50 700 Td (First PDF page) Tj ET" : ""),
    page(7), stream("0 0 50 50 re f"),
    page(9), stream(textPages ? "BT /F1 12 Tf 50 700 Td (Third PDF page) Tj ET" : ""),
    "<< /Type /Outlines /First 11 0 R /Last 12 0 R /Count 2 >>",
    "<< /Title (Textless page) /Parent 10 0 R /Dest [6 0 R /Fit] /Next 12 0 R >>",
    "<< /Title (Last page) /Parent 10 0 R /Dest [8 0 R /Fit] /Prev 11 0 R >>",
  ];
  let pdf = "%PDF-1.4\n";
  const offsets = objects.map((object, index) => {
    const offset = pdf.length;
    pdf += `${index + 1} 0 obj\n${object}\nendobj\n`;
    return `${String(offset).padStart(10, "0")} 00000 n \n`;
  });
  const xref = pdf.length;
  pdf += `xref\n0 ${objects.length + 1}\n0000000000 65535 f \n${offsets.join("")}trailer\n<< /Size ${objects.length + 1} /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF\n`;
  return new File([pdf], textPages ? "legacy.pdf" : "legacy-textless.pdf", { type: "application/pdf" });
};
