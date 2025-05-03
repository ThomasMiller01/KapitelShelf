export const UrlToFile = async (url: string): Promise<File> => {
  const fileName = url.split("/").pop() ?? "cover.png";

  const res = await fetch(url);
  const blob = await res.blob();

  return new File([blob], fileName, { type: blob.type });
};
