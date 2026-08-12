// src/app/router.ts
let navigateFn: (path: string) => void;

export function setNavigate(fn: (path: string) => void) {
  navigateFn = fn;
}

export function navigate(path: string) {
  if (navigateFn) navigateFn(path);
}
