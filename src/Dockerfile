
# ------ STAGE 1 (build container) ---------------------

# Source image to start with
FROM mono:6.8.0 as build-env

# Copy all the source and folder contents into the filesystem of the container
ADD M2Mqtt.Net.dll /
ADD Newtonsoft.Json.dll /
ADD PingPong.cs /

# FOR TEST ONLY
#RUN mkdir /cfg-data
#ADD mqtt-config.json /cfg-data

# Build solution
RUN mcs /reference:M2Mqtt.Net.dll /reference:Newtonsoft.Json.dll PingPong.cs

# ------ STAGE 2 (application container) -----------------

FROM mono:6.8.0
FROM python:3.7-alpine

#install paho-mqtt client in container
RUN pip install paho-mqtt

#copy just the compiled application and library from the previous stage into this new stage
COPY --from=build-env ./ /

# Run exe
CMD [ "mono", "./PingPong.exe"]