declare const __APP_VERSION__: string;

interface VersionInfo {
  APP_VERSION: string;
  BACKEND_VERSION: string;
}

export const useVersion = (): VersionInfo => {
  // do a rest request to /version
  const __BACKEND_VERSION__ = "TODO";
  return {
    APP_VERSION: __APP_VERSION__,
    BACKEND_VERSION: __BACKEND_VERSION__,
  };
};
