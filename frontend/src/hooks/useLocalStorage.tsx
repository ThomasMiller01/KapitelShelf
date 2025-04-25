const BASE_LOCAL_STORAGE_KEY = "kapitelshelf.";

export const useLocalStorage = (): [
  (key: string) => string | null,
  (key: string, value: string) => void
] => {
  const getItem = (key: string): string | null =>
    localStorage.getItem(BASE_LOCAL_STORAGE_KEY + key);

  const setItem = (key: string, value: string): void =>
    localStorage.setItem(BASE_LOCAL_STORAGE_KEY + key, value);

  return [getItem, setItem];
};
