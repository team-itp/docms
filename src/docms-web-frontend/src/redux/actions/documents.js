export const DOCUMENT_REQUEST = 'DOCUMENT_REQUEST';
export const DOCUMENT_RECEIVED = 'DOCUMENT_RECEIVED';
export const DOCUMENT_NOT_EXISTS = 'DOCUMENT_NOT_EXISTS';

export function requestDocument(path) {
  return { type: DOCUMENT_REQUEST, payload: { path: path } };
}
export function documentReceived({ path, ...data }) {
  return { type: DOCUMENT_REQUEST, payload: { path, ...data } };
}
export function documentNotExists(path) {
  return { type: DOCUMENT_REQUEST, payload: { path: path } };
}

export function fetchDocument(path) {
  return dispatch => {
    dispatch(requestDocument(path));
    return fetch('/api/docs' + path + 'index.js')
      .then(response => response.json())
      .then(json => documentReceived(json));
  }
}