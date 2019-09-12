import { DOCUMENT_REQUEST, DOCUMENT_RECEIVED } from "../actions/documents";

const initialState = {
  documents: {}
}

function documentsReducer(state = initialState, action) {
  switch (action.type) {
    case DOCUMENT_REQUEST:
      return Object.assign({}, state, {
        [action.path]: {
          isRequesting: true
        }
      });
    case DOCUMENT_RECEIVED:
      return Object.assign({}, state, {
        [action.path]: {
          isRequesting: false,
          type: action.payload.type,
          name: action.payload.name,
          documents: action.payload.documents,
        }
      });
    default:
      return state;
  }
}

export default documentsReducer;