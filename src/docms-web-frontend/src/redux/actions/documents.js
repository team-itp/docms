export const REQUEST_DOCUMENT = 'REQUEST_DOCUMENT';
export const SET_DOCUMENT = 'SET_DOCUMENT';
export const DOCUMENT_NOT_FOUND = 'DOCUMENT_NOT_FOUND';

export function requestDocument(path) {
  return { type: REQUEST_DOCUMENT, payload: { path: path } };
}
export function setDocument(data) {
  return { type: SET_DOCUMENT, payload: { ...data } };
}
export function documentNotFound(error, path) {
  return { type: DOCUMENT_NOT_FOUND, payload: { path, error } };
}
