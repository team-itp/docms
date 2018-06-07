import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Search } from './components/Search';
import { Documents } from './components/Documents';

export const routes = <Layout>
    <Route exact path='/' component={Search} />
    <Route path='/documents/:id' component={Documents} />
</Layout>;
