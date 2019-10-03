import { DOCUMENT_REQUEST, SET_DOCUMENT } from "../actions/documents";

const initialState = {
}

function documentsReducer(state = initialState, action) {
  switch (action.type) {
    case DOCUMENT_REQUEST:
      return Object.assign({}, state, {
        [action.payload.path]: {
          isRequesting: true
        }
      });
    case SET_DOCUMENT:
      return Object.assign({}, state, {
        [action.payload.path]: {
          isRequesting: false,
          type: action.payload.type,
          name: action.payload.name,
          path: action.payload.path,
          entries: action.payload.entries,
        }
      });
    default:
      return state;
  }
}

export default documentsReducer;