
# ------ STAGE 1 (build container) ---------------------

# Source image to start with
FROM mono:6.8.0 as build-env

# create working directory for build
ADD . /build
WORKDIR /build

# install git and nuget
RUN apt update && apt install -y git nuget

# Create DLL M2Mqtt.Net.dll via GitHub Repo (all in one container):
# - download MQTT client library
# - change directory
# - build M2Mqtt.Net.dll and save output files in dir "build" of container
RUN git clone https://github.com/eclipse/paho.mqtt.m2mqtt \
&& cd paho.mqtt.m2mqtt/M2Mqtt \
&& msbuild M2Mqtt.Net.csproj /p:Configuration=Release /p:OutputPath=/build

# Create DLL Newtonsoft.Json.dll via NuGet package:
# - install nuget package in directory "build"
# - copy Newtonsoft.Json.dll into directory "build"
RUN nuget install Newtonsoft.Json -Version 12.0.3 -OutputDirectory /build
RUN cd Newtonsoft.Json.12.0.3/lib/net40 && cp Newtonsoft.Json.dll /build

# Copy source file into the filesystem of the container
ADD PingPong.cs .

# Build solution
RUN mcs /reference:M2Mqtt.Net.dll /reference:Newtonsoft.Json.dll PingPong.cs

# ------ STAGE 2 (application container) -----------------

FROM mono:6.8.0

# create working directory
ADD . /pingpong
WORKDIR /pingpong

# install nuget
RUN apt update && apt install -y nuget

# Install mqtt client in container
RUN nuget install M2Mqtt

#copy just the compiled application and libraries from the previous stage into this stage
COPY --from=build-env /build .

# Run exe
CMD [ "mono", "./PingPong.exe"]
