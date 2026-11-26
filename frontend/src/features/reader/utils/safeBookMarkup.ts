const readable = new Set("a abbr b bdi bdo blockquote br caption cite code dd del details div dl dt em figcaption figure h1 h2 h3 h4 h5 h6 hr i img ins kbd li mark ol p pre q rp rt ruby s samp section small span strong sub summary sup table tbody td th thead tfoot tr u ul var wbr".split(" "));
const discard = new Set(["applet", "audio", "embed", "form", "iframe", "math", "noscript", "object", "script", "style", "template", "video"]);
const attributes = ["id", "class", "title", "dir", "lang", "alt", "width", "height", "colspan", "rowspan", "start", "value", "aria-label"];
const textStyles = ["text-align", "font-style", "font-weight", "text-decoration", "vertical-align", "white-space"];

const imageSource = (href: string | null): href is string =>
  href !== null && (/^blob:/i.test(href) || /^data:image\/(?:png|jpeg|gif|webp|bmp|avif);base64,/i.test(href));

const clean = (node: Node, parent: HTMLElement | DocumentFragment): void => {
  if (node.nodeType === Node.TEXT_NODE) {
    parent.append(document.createTextNode(node.textContent ?? ""));
    return;
  }
  if (!(node instanceof Element) || discard.has(node.localName)) {
    return;
  }

  if (node.localName === "svg") {
    for (const embedded of node.querySelectorAll("image")) {
      const href = embedded.getAttribute("href") ?? embedded.getAttribute("xlink:href");
      if (imageSource(href)) {
        const image = document.createElement("img");
        image.src = href;
        image.alt = embedded.getAttribute("aria-label") ?? node.querySelector("title")?.textContent ?? "";
        parent.append(image);
      }
    }
    return;
  }

  if (!readable.has(node.localName)) {
    node.childNodes.forEach((child) => clean(child, parent));
    return;
  }
  const element = document.createElement(node.localName);
  for (const name of attributes) {
    const value = node.getAttribute(name);
    if (value !== null) {
      element.setAttribute(name, value);
    }
  }
  if (node instanceof HTMLElement) {
    for (const property of textStyles) {
      const value = node.style.getPropertyValue(property);
      if (value) {
        element.style.setProperty(property, value);
      }
    }
  }

  if (element instanceof HTMLImageElement) {
    const src = node.getAttribute("src");
    if (imageSource(src)) {
      element.src = src;
    } else {
      parent.append(document.createTextNode(element.alt));
      return;
    }
  }
  if (element instanceof HTMLAnchorElement) {
    const href = node.getAttribute("href")?.trim();
    if (href?.startsWith("#")) {
      element.href = href;
    } else if (href && /^(?:https?:\/\/|mailto:)/i.test(href)) {
      element.href = href;
      element.target = "_blank";
      element.rel = "noopener noreferrer";
    } else if (href && /^[^:/\\?#]+(?:\/[^:/\\?#]+)*(?:#[^#]*)?$/.test(href)) {
      element.href = "#";
      element.dataset.bookLink = href;
    }
  }

  node.childNodes.forEach((child) => clean(child, element));
  parent.append(element);
};

export const safeBookMarkup = (html: string): DocumentFragment => {
  const input = document.createElement("template");
  input.innerHTML = html;
  const output = document.createDocumentFragment();
  input.content.childNodes.forEach((node) => clean(node, output));
  return output;
};
