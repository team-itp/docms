import React from 'react';
import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { requestDocument } from '../redux/actions/documents';
import { DocumentViewer, DirectoryViewer } from '../components/DocumentViewer';

function getCurrentEntry(documents, path) {
  return documents[path];
}

function mapStateToProps(state, props) {
  const path = props.location.pathname.replace(/^\/docs\/?/, '').replace(/\/$/, '');
  const pathComponents = (path || '').split('/');
  const name = pathComponents.pop();
  return {
    path: path,
    name: name,
    entry: getCurrentEntry(state.documents, path)
  };
}

function mapDispatchToProps(dispatch, props) {
  const { history } = props;
  return {
    onSelectPath: path => {
      dispatch(requestDocument(path));
      history.push('/docs/' + path);
    }
  };
}

class DocumentBrowser extends React.Component {

  componentDidMount() {
    this.props.onSelectPath(this.props.path);
  }

  render() {
    const { entry } = this.props;
    if (!entry || entry.isFetching) {
      return null;
    }
    return (
      entry.type === 'DIRECTORY'
        ? <DirectoryViewer onSelectPath={this.props.onSelectPath} {...entry} />
        : <DocumentViewer onSelectPath={this.props.onSelectPath} {...entry} />
    );
  }
}

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(DocumentBrowser));