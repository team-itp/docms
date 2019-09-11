import React from 'react';
import { Link as RouterLink, withRouter } from 'react-router-dom';
import { Breadcrumbs, Container, Link, Typography, Toolbar, Paper, Grid } from '@material-ui/core';

class DocumentBrowser extends React.Component {

  getStateFromPath({ location }) {
    const path = location.pathname.replace(/^\/docs\//, '');
    const pathComponents = (path || '').split('/');
    const name = pathComponents.pop();
    const parentPath = pathComponents.join('/');
    return {
      type: 'UNKNOWN',
      path: location.pathname,
      parentPath: parentPath,
      name: name
    };
  }

  render() {
    let state = this.getStateFromPath(this.props);
    let documentList = this.props.documentList || [];
    return (
      <Container maxWidth="xl">
        <DocumentHeader parentPath={state.parentPath} name={state.name} />
        {state.type === 'DIRECTORY'
          ? documentList.map(e => <DocumentSummary name={e.name} />)
          : <DocumentViewer {...this.props} {...state} />}
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

export default withRouter(DocumentBrowser);