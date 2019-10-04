import React from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Breadcrumbs, Container, Link, Typography, Toolbar, Paper, Grid, List, ListItem, ListItemText } from '@material-ui/core';

function getLink(path) {
  return '/docs/' + path;
}

export function DocumentHeader(props) {
  let path = props.path;
  let linkPath = '';
  let links = (path || '').split('/').filter(e => e).map((e, i) => {
    linkPath = linkPath ? linkPath + '/' + e : e;
    return {
      key: i,
      path: linkPath,
      name: e
    }
  });
  return (
    <Toolbar>
      <Breadcrumbs aria-label="breadcrumb" >
        <Link color="inherit" onClick={() => props.onSelectPath('')}>Home</Link>
        {links.map(e => (<Link key={e.key} color="inherit" component={RouterLink} onClick={() => props.onSelectPath(e.path)} to={getLink(e.path)}>{e.name}</Link>))}
        {props.name && <Typography color="textPrimary">{props.name}</Typography>}
      </Breadcrumbs>
    </Toolbar>
  );
}

export function DocumentViewer(props) {
  return (
    <Container maxWidth="xl">
      <DocumentHeader {...props} />
      <Container>
        <Paper>
          <Grid>
            <Typography>{props.name}</Typography>
          </Grid>
        </Paper>
      </Container>
    </Container>
  );
}

export function DirectoryViewer(props) {
  const { entries } = props;
  return (
    <Container maxWidth="xl">
      <DocumentHeader  {...props} />
      {(entries && (entries.length > 0))
        ? (<List>{entries.map(e => <DocumentSummary key={e.name} {...e} onSelectPath={props.onSelectPath} />)}</List>)
        : null}
    </Container>
  );
}

export function DocumentSummary(props) {
  return (<ListItem
    button
    onClick={_ => props.onSelectPath(props.path)}>
    <ListItemText primary={props.name} />
  </ListItem>);
}