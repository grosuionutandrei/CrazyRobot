import { useWsClient } from 'ws-request-hook';
import {ClientIsDisconnectedDto, ServerSendsErrorMessageDto, StringConstants} from '../../api';
import { useClientIdState } from './hooks.ts';
import { KEYS } from '../KEYS';
import toast from "react-hot-toast";

export const useWebsocketDisconnect = () => {
    const wsClient = useWsClient();
    const manageClientId = useClientIdState(KEYS.CLIENT_ID);

    const disconnectWebsocket = async () => {
        try {
            if (wsClient && wsClient.readyState === 1) { // Check if connection is open
                const clientId = manageClientId.getClientId();
                if (clientId) {
                    const disconnectRequest: ClientIsDisconnectedDto = {
                        clientId: clientId,
                        eventType: StringConstants.ClientIsDisconnectedDto,
                        requestId: crypto.randomUUID()
                    };

                    await wsClient.send(disconnectRequest);
                }
            }
        } catch (error) {
            const errorDto = error as unknown as ServerSendsErrorMessageDto;
            toast.error("ErrorReceived: " + errorDto.message!);
        }
    };

    return { disconnectWebsocket };
};