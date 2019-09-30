import { DOCUMENT_REQUEST, SET_DOCUMENT } from "../actions/documents";

const initialState = {
}

function documentsReducer(state = initialState, action) {
  switch (action.type) {
    case DOCUMENT_REQUEST:
      return Object.assign({}, state, {
        [action.path]: {
          isRequesting: true
        }
      });
    case SET_DOCUMENT:
      return Object.assign({}, state, {
        [action.path]: {
          isRequesting: false,
          type: action.payload.type,
          name: action.payload.name,
          entries: action.payload.entries,
        }
      });
    default:
      return state;
  }
}

export default documentsReducer;