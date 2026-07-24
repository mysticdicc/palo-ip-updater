# Palo Alto Fw IP Address Changer (PAFIC)
At home I run a Palo Alto firewall as my edge device. Palo alto devices allow for the storing of IP addresses as named objects that you can use to create network rules. To automate my dynamic DNS setup I have created this small docker container that automatically SSHs to the firewall, updates a named IP object to your current public IP address and then goes back to sleep. Setup is simple,
there is a small configuration file to create and then a single docker command will have you up and running.

## Get Started (Linux)
1. Make a folder to store your configuration file: 
```
mkdir ~/palo
```
2. Make an appsettings.json file: 
```
nano ~/palo/appsettings.json
```
3. Paste the below into the configuration file, ensuring to update the caps fields to the details for your Palo device. Keep all quotes and do not add quotes around the number for update interval: 
```
{
  "PaloLoginDetails": {
    "Username": "SSH_USERNAME",
    "Password": "SSH_PASSWORD",
    "HostIp": "FIREWALL_IP",
    "HostIpName": "PUBLIC_IP_OBJECT_NAME",
    "UpdateInterval":  UPDATE_INTERVAL_SECONDS
  }
}
```
4. Save file
5. Run docker command: 
```
docker run -d \
--name=CONTAINER_NAME
-v /home/username/palo/appsettings.json:/app/appsettings.json \
--restart unless-stopped \
ghcr.io/mysticdicc/palo-ip-updater:latest
```
You can view the container logs to see the services progress and any errors.
