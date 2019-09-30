export const DOCUMENT_REQUEST = 'DOCUMENT_REQUEST';
export const SET_DOCUMENT = 'SET_DOCUMENT';
export const DOCUMENT_REQUEST_FAIL = 'DOCUMENT_REQUEST_FAIL';

export function requestDocument(path) {
  return { type: DOCUMENT_REQUEST, payload: { path: path } };
}
export function documentReceived({ path, ...data }) {
  return { type: SET_DOCUMENT, payload: { path, ...data } };
}
export function documentNotExists(path) {
  return { type: DOCUMENT_REQUEST_FAIL, payload: { path: path } };
}
