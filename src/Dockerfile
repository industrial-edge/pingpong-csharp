
# ------ STAGE 1 (build container) ---------------------

# Source image to start with
FROM mono:6.8.0 as build-env

# create directory for build results
ADD . /results
WORKDIR /results

# install git and nuget
RUN apt update && apt install -y git nuget

# Create DLL M2Mqtt.Net.dll via GitHub Repo (all in one container):
# - download MQTT client library
# - change directory
# - build M2Mqtt.Net.dll and save output files in dir "results" of container
RUN git clone https://github.com/eclipse/paho.mqtt.m2mqtt \
&& cd paho.mqtt.m2mqtt/M2Mqtt \
&& msbuild M2Mqtt.Net.csproj /p:Configuration=Release /p:OutputPath=./../..

# Create DLL Newtonsoft.Json.dll via NuGet package:
# - install nuget package in directory "results"
# - copy Newtonsoft.Json.dll into directory "results"
RUN nuget install Newtonsoft.Json -OutputDirectory results
RUN nuget install Newtonsoft.Json
RUN cd Newtonsoft.Json.12.0.3/lib/net40 && cp Newtonsoft.Json.dll ./../../..

# Copy source file into the filesystem of the container
ADD PingPong.cs .

# Build solution
RUN mcs /reference:M2Mqtt.Net.dll /reference:Newtonsoft.Json.dll PingPong.cs


# ------ STAGE 2 (application container) -----------------

FROM mono:6.8.0
FROM python:3.7-alpine

#install paho-mqtt client in container
RUN pip install paho-mqtt

#copy data from the previous stage into this new stage
COPY --from=build-env . .

# Run exe
CMD [ "mono", "/results/PingPong.exe"]
