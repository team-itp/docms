import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import { DocumentDetail } from './DocumentDetail'


interface DocumentsState {
    id: number
}

export class Documents extends React.Component<RouteComponentProps<{}>, DocumentsState> {
    public render() {
        const {params} = this.props.match
        return <DocumentDetail documentId={1} />
    }
}
