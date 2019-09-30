import React from 'react';
import { connect } from 'react-redux';
import { Link as RouterLink, withRouter } from 'react-router-dom';
import { Breadcrumbs, Container, Link, Typography, Toolbar, Paper, Grid } from '@material-ui/core';
import { requestDocument } from '../redux/actions/documents';

function getCurrentEntry(documents, path) {
  return documents[path];
}

function mapStateToProps(state, props) {
  const path = props.location.pathname.replace(/^\/docs\//, '');
  const pathComponents = (path || '').split('/');
  const name = pathComponents.pop();
  const parentPath = pathComponents.join('/');
  return {
    path: path,
    parentPath: parentPath,
    name: name,
    entry: getCurrentEntry(state.documents, props.path)
  };
}

function mapDispatchToProps(dispatch, props) {
  return {
    onSelectPath: path => dispatch(requestDocument(path))
  };
}

class DocumentBrowser extends React.Component {

  componentDidMount() {
    this.props.onSelectPath(this.props.path);
  }

  selectPath(path) {
    this.props.onSelectPath(path);
  }

  render() {
    const { parentPath, name, entry } = this.props;
    return (
      <Container maxWidth="xl">
        <DocumentHeader parentPath={parentPath} name={name} />
        {(!entry || entry.isFetching)
          ? null
          : entry.type === 'DIRECTORY'
            ? entry.entries.map(e => <DocumentSummary name={e.name} />)
            : <DocumentViewer {...entry} />}
      </Container>
    );
  }
}

function DocumentHeader(props) {
  let path = props.parentPath;
  let linkPath = '/docs'
  let links = (path || '').split('/').filter(e => e).map((e, i) => {
    linkPath = linkPath + '/' + e;
    return {
      key: i,
      path: linkPath,
      name: e
    }
  });
  return (
    <Toolbar>
      <Breadcrumbs aria-label="breadcrumb" >
        <Link color="inherit" component={RouterLink} to="/docs/">Home</Link>
        {links.map(e => (<Link key={e.key} color="inherit" component={RouterLink} to={e.path}>{e.name}</Link>))}
        {props.name && <Typography color="textPrimary">{props.name}</Typography>}
      </Breadcrumbs>
    </Toolbar>
  );
}

function DocumentSummary(props) {
  return (
    <Paper>
      <Grid>
        <Typography>{props.name}</Typography>
      </Grid>
    </Paper>
  )
}

function DocumentViewer(props) {
  return (
    <Paper>
      <Grid>
        <Typography>{props.name}</Typography>
      </Grid>
    </Paper>
  )
}

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(DocumentBrowser));