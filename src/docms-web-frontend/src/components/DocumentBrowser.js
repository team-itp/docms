import React from 'react';
import { connect } from 'react-redux';
import { Link as RouterLink, withRouter } from 'react-router-dom';
import { Breadcrumbs, Container, Link, Typography, Toolbar, Paper, Grid } from '@material-ui/core';
import { fetchDocument } from '../redux/actions/documents';

class DocumentBrowser extends React.Component {

  componentDidMount() {
    const path = this.props.location.pathname.replace(/^\/docs\//, '');
    this.props.onSelectPath(path)
  }

  selectPath(path) {
    this.props.onSelectPath(path);
  }

  render() {
    const path = this.props.location.pathname.replace(/^\/docs\//, '');
    const pathComponents = (path || '').split('/');
    const name = pathComponents.pop();
    const parentPath = pathComponents.join('/');
    let documentList = this.props.documentList || [];
    return (
      <Container maxWidth="xl">
        <DocumentHeader parentPath={parentPath} name={name} />
        {this.props.type === 'DIRECTORY'
          ? documentList.map(e => <DocumentSummary name={e.name} />)
          : <DocumentViewer {...this.props} />}
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

function mapStateToProps(state, props) {
  const docInfo = state.documents[props.path] || {};
  return {
    isFetching: docInfo.isFetching
  };
}

function mapDispatchToProps(dispatch, props) {
  return {
    onSelectPath: path => dispatch(fetchDocument(props.path))
  };
}

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(DocumentBrowser));